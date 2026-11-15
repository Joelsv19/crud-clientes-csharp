import html
import http.cookiejar
import json
import re
import sys
import uuid
from datetime import datetime
from html.parser import HTMLParser
from pathlib import Path
from urllib.error import HTTPError
from urllib.parse import urlencode
from urllib.request import HTTPCookieProcessor, HTTPRedirectHandler, Request, build_opener


class SinRedireccion(HTTPRedirectHandler):
    def redirect_request(self, req, fp, code, msg, headers, newurl):
        return None


class Formulario(HTMLParser):
    def __init__(self, pagina):
        super().__init__()
        self.valores = {}
        self.selecciones = {}
        self.selector = None
        self.feed(pagina)

    def handle_starttag(self, tag, attrs):
        datos = dict(attrs)
        if tag == 'input' and 'name' in datos:
            self.valores[datos['name']] = datos.get('value', '')
        if tag == 'select':
            self.selector = datos.get('name')
        if tag == 'option' and 'selected' in datos:
            self.selecciones[self.selector] = datos.get('value', '')

    def handle_endtag(self, tag):
        if tag == 'select':
            self.selector = None


BASE = sys.argv[1] if len(sys.argv) > 1 else 'http://localhost:5183'
cliente_http = build_opener(HTTPCookieProcessor(http.cookiejar.CookieJar()), SinRedireccion())
resultados = []
creados = []
marca = 'PruebaSemana3_' + uuid.uuid4().hex[:8]
datos = {
    'Nombres': marca, 'Apellidos': 'Cliente de prueba', 'Cedula': '0000000099',
    'Correo': 'prueba@example.com', 'Telefono': '0990000099',
    'Direccion': 'Dirección ficticia de prueba', 'FechaNacimiento': '2001-02-03',
    'Ocupacion': 'Estudiante', 'ProvinciaId': '1', 'CantonId': '2'
}


def comprobar(nombre, condicion):
    resultados.append({'prueba': nombre, 'correcta': bool(condicion)})
    print(('OK' if condicion else 'FALLO') + ': ' + nombre, flush=True)
    if not condicion:
        raise AssertionError(nombre)


def solicitar(ruta, campos=None):
    peticion = Request(BASE + ruta, data=urlencode(campos).encode() if campos is not None else None)
    try:
        respuesta = cliente_http.open(peticion, timeout=20)
    except HTTPError as error:
        respuesta = error
    return respuesta.code, respuesta.read().decode('utf-8'), respuesta.headers


def enviar(ruta, campos, formulario='/Clientes/Create'):
    codigo, pagina, _ = solicitar(formulario)
    assert codigo == 200
    token = Formulario(pagina).valores['__RequestVerificationToken']
    return solicitar(ruta, {**campos, '__RequestVerificationToken': token})


def buscar_id(nombre):
    _, listado, _ = solicitar('/Clientes')
    filas = re.findall(r'<tr>(.*?)</tr>', listado, re.S)
    fila = next(fila for fila in filas if nombre in html.unescape(fila))
    return int(re.search(r'/Clientes/Details/(\d+)', fila).group(1))


try:
    codigo, pagina, _ = solicitar('/')
    comprobar('Inicio y encabezado en español', codigo == 200 and 'Gestión de clientes' in pagina)
    for recurso in ['/css/site.css', '/js/clientes.js', '/lib/jquery/dist/jquery.min.js',
                    '/lib/jquery-validation/dist/jquery.validate.min.js',
                    '/lib/jquery-validation-unobtrusive/dist/jquery.validate.unobtrusive.min.js']:
        comprobar('Recurso local: ' + recurso, solicitar(recurso)[0] == 200)

    for provincia, ids in [(1, {1, 2, 3}), (2, {4, 5, 6}), (3, {7, 8, 9}), (4, {10, 11, 12})]:
        codigo, contenido, _ = solicitar('/Clientes/GetCantones?' + urlencode({'provinciaId': provincia}))
        comprobar('Cantones filtrados de provincia ' + str(provincia),
                  codigo == 200 and {c['id'] for c in json.loads(contenido)} == ids)
    for provincia in ['0', '-1', '999999', 'abc']:
        codigo, contenido, _ = solicitar('/Clientes/GetCantones?provinciaId=' + provincia)
        comprobar('Consulta de provincia inválida ' + provincia, codigo == 200 and json.loads(contenido) == [])

    vacios = {campo: '' for campo in datos}
    codigo, pagina, _ = enviar('/Clientes/Create', vacios)
    comprobar('Rechazo de los diez campos vacíos', codigo == 200 and 'validation-summary-errors' in pagina
              and all(re.search('data-valmsg-for="' + campo + '"[^>]*>[^<]+', pagina) for campo in datos))

    casos_invalidos = [
        ('Correo incorrecto', {'Correo': 'correo-invalido'}, 'correo electrónico válido'),
        ('Cédula incorrecta', {'Cedula': '123'}, '10 dígitos'),
        ('Teléfono incorrecto', {'Telefono': 'abc'}, 'teléfono de 7 a 20'),
        ('Fecha futura', {'FechaNacimiento': '2999-01-01'}, 'no puede ser futura'),
        ('Fecha no interpretable', {'FechaNacimiento': 'fecha-incorrecta'}, 'validation-summary-errors'),
        ('Nombre mayor al máximo', {'Nombres': 'A' * 81}, 'validation-summary-errors'),
        ('Cantón de otra provincia', {'ProvinciaId': '1', 'CantonId': '4'}, 'pertenezca'),
        ('Provincia inexistente', {'ProvinciaId': '999999'}, 'no existe'),
        ('Cantón inexistente', {'CantonId': '999999'}, 'pertenezca'),
    ]
    for nombre, cambios, mensaje in casos_invalidos:
        codigo, pagina, _ = enviar('/Clientes/Create', {**datos, **cambios})
        comprobar(nombre, codigo == 200 and mensaje in html.unescape(pagina))

    codigo, _, _ = enviar('/Clientes/Create', {**datos, 'Id': '2147483000'})
    comprobar('Registro completo por formulario POST', codigo == 302)
    cliente_id = buscar_id(marca)
    creados.append(cliente_id)
    comprobar('ID generado por la base y protegido de sobreasignación', 0 < cliente_id < 2147483000)

    ruta_detalles = '/Clientes/Details/' + str(cliente_id)
    codigo, pagina, _ = solicitar(ruta_detalles)
    texto = html.unescape(pagina)
    comprobar('Detalles de los diez campos', codigo == 200 and all(valor in texto for valor in
              [datos[c] for c in ['Nombres', 'Apellidos', 'Cedula', 'Correo', 'Telefono', 'Direccion', 'Ocupacion']]
              + ['03/02/2001', 'Pichincha', 'Rumiñahui']))
    comprobar('Persistencia tras nueva petición HTTP', solicitar(ruta_detalles)[1] == pagina)

    ruta_editar = '/Clientes/Edit/' + str(cliente_id)
    _, pagina, _ = solicitar(ruta_editar)
    formulario = Formulario(pagina)
    comprobar('Edición conserva los ocho datos de información personal',
              all(formulario.valores[c] == datos[c] for c in datos if c not in ['ProvinciaId', 'CantonId']))
    comprobar('Edición conserva provincia y cantón', formulario.selecciones == {'ProvinciaId': '1', 'CantonId': '2'})

    codigo, pagina, _ = enviar(ruta_editar, {**datos, 'Id': cliente_id, 'CantonId': '4'}, ruta_editar)
    comprobar('Edición rechaza provincia y cantón incompatibles', codigo == 200 and 'pertenezca' in html.unescape(pagina))
    codigo, pagina, _ = enviar(ruta_editar, {**datos, 'Id': cliente_id, 'Correo': 'mal'}, ruta_editar)
    formulario = Formulario(pagina)
    comprobar('Validación fallida conserva las selecciones', codigo == 200 and
              formulario.selecciones == {'ProvinciaId': '1', 'CantonId': '2'})

    cambios = {**datos, 'Id': cliente_id, 'Nombres': marca + '_Editado', 'Apellidos': 'Apellido actualizado',
               'Cedula': '0000000088', 'Correo': 'editado@example.com', 'Telefono': '0990000088',
               'Direccion': 'Nueva dirección ficticia', 'FechaNacimiento': '1999-06-15',
               'Ocupacion': 'Docente de prueba', 'ProvinciaId': '2', 'CantonId': '5'}
    comprobar('Guardar edición de los diez campos', enviar(ruta_editar, cambios, ruta_editar)[0] == 302)
    _, pagina, _ = solicitar(ruta_detalles)
    texto = html.unescape(pagina)
    comprobar('Cambios guardados y persistentes', all(valor in texto for valor in
              [cambios[c] for c in ['Nombres', 'Apellidos', 'Cedula', 'Correo', 'Telefono', 'Direccion', 'Ocupacion']]
              + ['15/06/1999', 'Guayas', 'Durán']))

    for accion in ['Details', 'Edit', 'Delete']:
        for identificador in ['999999999', 'abc', '']:
            comprobar('Registro inexistente GET ' + accion + '/' + identificador,
                      solicitar('/Clientes/' + accion + '/' + identificador)[0] == 404)
    comprobar('Registro inexistente POST Edit', enviar('/Clientes/Edit/999999999',
              {**datos, 'Id': '999999999'})[0] == 404)
    comprobar('Registro inexistente POST Delete', enviar('/Clientes/Delete/999999999', {'Id': '999999999'})[0] == 404)
    comprobar('ID de edición incoherente', enviar(ruta_editar, {**cambios, 'Id': cliente_id + 1}, ruta_editar)[0] == 404)

    for accion in ['Create', 'Edit', 'Delete']:
        ruta = '/Clientes/' + accion + ('/' + str(cliente_id) if accion != 'Create' else '')
        comprobar('Antifalsificación en ' + accion, solicitar(ruta, cambios)[0] == 400)

    ruta_eliminar = '/Clientes/Delete/' + str(cliente_id)
    codigo, pagina, _ = solicitar(ruta_eliminar)
    comprobar('Pantalla de confirmación', codigo == 200 and 'Confirmar eliminación' in pagina)
    comprobar('GET Delete no elimina el cliente', solicitar(ruta_detalles)[0] == 200)
    comprobar('Eliminación confirmada por POST', enviar(ruta_eliminar, {'Id': cliente_id}, ruta_eliminar)[0] == 302)
    creados.remove(cliente_id)
    comprobar('Eliminación persistente', solicitar(ruta_detalles)[0] == 404)
    comprobar('Cliente eliminado ya no aparece en listado', marca not in solicitar('/Clientes')[1])

    datos_escape = {**datos, 'Nombres': '<script>alert("prueba")</script>'}
    comprobar('Registro de texto especial para comprobar escape', enviar('/Clientes/Create', datos_escape)[0] == 302)
    escape_id = buscar_id(datos_escape['Nombres'])
    creados.append(escape_id)
    _, pagina, _ = solicitar('/Clientes/Details/' + str(escape_id))
    comprobar('Razor escapa HTML almacenado', '<script>alert(' not in pagina and '&lt;script&gt;' in pagina)
finally:
    for cliente_id in creados:
        ruta = '/Clientes/Delete/' + str(cliente_id)
        if solicitar(ruta)[0] == 200:
            enviar(ruta, {'Id': cliente_id}, ruta)
    salida = Path(__file__).resolve().parents[1] / '.local' / 'resultado-http.json'
    salida.parent.mkdir(exist_ok=True)
    salida.write_text(json.dumps({'fecha': datetime.now().astimezone().isoformat(), 'base': BASE,
                                 'comprobaciones': resultados}, ensure_ascii=False, indent=2), encoding='utf-8')
    print(str(sum(r['correcta'] for r in resultados)) + '/' + str(len(resultados)) + ' comprobaciones correctas.')
