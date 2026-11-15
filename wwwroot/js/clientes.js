$(function () {
    const provincia = $("#ProvinciaId");
    const canton = $("#CantonId");
    const estado = $("#estado-cantones");
    let solicitud;

    provincia.on("change", function () {
        if (solicitud) solicitud.abort();
        canton.empty().append(new Option("Seleccione un cantón", "")).prop("disabled", true);
        const provinciaId = provincia.val();
        if (!provinciaId) {
            estado.text("Seleccione una provincia para cargar sus cantones.");
            return;
        }

        estado.text("Cargando cantones…");
        solicitud = $.getJSON(provincia.data("cantones-url"), { provinciaId: provinciaId })
            .done(function (cantones) {
                $.each(cantones, function (_, item) {
                    canton.append(new Option(item.nombre, item.id));
                });
                canton.prop("disabled", cantones.length === 0);
                estado.text(cantones.length ? "Seleccione un cantón de la provincia elegida." : "La provincia no tiene cantones disponibles.");
            })
            .fail(function (_, estadoSolicitud) {
                if (estadoSolicitud !== "abort")
                    estado.text("No se pudieron cargar los cantones. Seleccione nuevamente la provincia para reintentar.");
            });
    });
});
