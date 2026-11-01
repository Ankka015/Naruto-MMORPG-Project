# Konoha: plano maestro y estado de construccion

## Referencia y orientacion

La ilustracion general compartida en el chat es el plano maestro de la composicion. El mundo usa **norte = +Z**, **sur = -Z**, **este = +X** y **oeste = -X**. La escena debe conservar el monumento al norte, la puerta principal al sur, el edificio del Hokage en el centro y los distritos alrededor. La imagen del monumento ya importada en `Assets/Konoha/Textures/HokageRockReferenceV2.png` es su referencia visual directa.

La ilustracion es una imagen 2D; el objetivo de la escena es conservar su distribucion y siluetas desde la vista aerea y traducirlas a un entorno 3D transitable. Las casas y edificios actuales son modelos provisionales generados con primitivas; aun no reproducen cada detalle del arte final.

## Plano de ubicaciones actual

| Zona | Centro aproximado (X, Z) | Relacion con el plano |
|---|---:|---|
| Roca de los Hokage | (0, 230) | Borde norte |
| Oficina del Hokage | (0, 80) | Centro urbano |
| Hospital | (76, 28) | Este del Hokage |
| Arena | (-152, -105) | Oeste y al sur del centro |
| Distrito comercial | (0, -165) | Franja sur central |
| Distrito industrial | (190, -205) | Sureste |
| Puerta principal | (0, -278) | Muro sur |
| Cascada | (356, -48) | Exterior este |

Los ocho complejos de clanes quedan asi:

| Clan | Centro aproximado (X, Z) | Sector |
|---|---:|---|
| Inuzuka | (-205, 180) | Noroeste |
| Nara | (-68, 184) | Norte, hacia el noroeste |
| Aburame | (70, 184) | Norte central |
| Hyuga | (207, 180) | Noreste / este |
| Yamanaka | (-205, 56) | Oeste |
| Sarutobi | (-90, 55) | Oeste central |
| Uchiha | (205, 56) | Este |
| Akimichi | (-205, -190) | Suroeste |

El mapa de referencia dibuja siete clanes; Akimichi se agrega en el suroeste para respetar la lista de ocho clanes del prompt sin mover los puntos de referencia principales.

## Auditoria del proyecto

- Motor: Unity 6.6.6f1, Universal Render Pipeline 17.6.0.
- Escena de Konoha: `Assets/Scenes/KonohaVillage.unity`; incluida en la lista de compilacion.
- Generador: `Assets/Editor/KonohaSceneBuilder.cs`; version 21, reconstruye y guarda la escena completa cuando Unity importa el script.
- Estado de esta auditoria: el marcador y la escena siguen en version 20; la version 21 esta escrita, pero Unity aun no la ha importado ni reconstruido.
- Control actual: `KonohaWalker.cs` usa `CharacterController` y WASD; la camara sigue al jugador y el mouse gira horizontalmente.
- Recursos existentes: dos imagenes de la Roca de los Hokage, texturas/materiales procedurales de estilo anime, cinco shaders y un prefab de arbol. No hay modelos externos FBX/OBJ/GLTF en `Assets`.
- La escena actual se compone principalmente de primitivas, colisionadores simples y mallas creadas por el generador. No hay interiores detallados ni NavMesh de NPCs.

## Estado por etapas

1. **Geografia y terreno — parcial.** Terreno amplio con canal de rio; falta dar relieve de valle y laderas alrededor de toda la aldea.
2. **Montanas, bosque y agua — parcial.** Roca de los Hokage con imagen, bosque perimetral, rio animado y cascada; falta modelar una cordillera y transiciones de altura naturales.
3. **Murallas y puertas — base implementada.** Muros, torres y puertas sur, este y oeste. Falta revisar su silueta y recorrido frente al plano maestro.
4. **Distribucion de distritos — base implementada.** Hokage, hospital, arena, comercio, residencial e industrial. El generador version 21 agrega el complejo Akimichi y alinea las ocho regiones de clanes; la escena guardada aun espera esa reconstruccion.
5. **Calles y avenidas — parcial.** Avenida principal, ejes de acceso, cruces de rio y calle comercial; falta una red secundaria mas organica y conectada.
6. **Edificios principales — bloqueos funcionales.** Oficina del Hokage, hospital, academia, arena y comercios; falta reemplazar las formas provisionales por modelos modulares y crear interiores.
7. **Complejos de clanes — parcial.** La escena version 20 contiene siete parcelas. El generador version 21 incluye la octava, Akimichi, con emblema y detalles propios; falta que Unity la regenere y luego completar la arquitectura final de cada clan.
8. **Barrios civiles — bloqueos procedurales.** Casas variadas por color y tejado; falta mayor detalle, patios, callejones e interiores.
9. **Infraestructura militar — bloqueos procedurales.** ANBU, policia, misiones y barracas; faltan funciones de juego y edificios finales.
10. **Detalles y rendimiento — parcial.** Texturas estilizadas, agua y vegetacion animadas; falta optimizar la cantidad de objetos, revisar colisiones, preparar LOD/instancing y validar el rendimiento en el editor y en Play.

## Secuencia de trabajo

1. Fijar y revisar la distribucion aerea, incluyendo las ocho parcelas de clanes.
2. Completar la geografia del valle con alturas y laderas, manteniendo libres las rutas y el canal del rio.
3. Refinar murallas, accesos, puentes y conectividad vial.
4. Sustituir los bloqueos por modulos 3D propios, conservar y reutilizar las texturas existentes, y completar edificios e interiores.
5. Mejorar movimiento, colisiones y navegacion; optimizar mallas y vegetacion.

El generador reconstruye solo la escena objetivo al cambiar su version; no elimina los otros archivos ni las escenas del proyecto. Antes de convertir bloques en modelos finales, mantener el plano de esta pagina como referencia de posiciones.
