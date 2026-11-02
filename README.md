Sudoku WinForms

Juego de Sudoku de escritorio para Windows, hecho con C# 14 / .NET 10 y Windows Forms (sin librerías de UI de terceros). Genera puzzles de solución única en cuatro dificultades, valida las reglas en vivo, permite anotar candidatos, deshacer y rehacer, pedir pistas, guardar y reanudar partidas, consultar estadísticas y cambiar entre tema claro y oscuro.

El proyecto también es un ejercicio de arquitectura limpia, SOLID y DRY aplicado a una aplicación de escritorio.

Características
Tablero 9x9 dibujado con GDI+ en un control propio, siempre cuadrado, centrado y sin dependencia del DPI.
Generador de puzzles con cuatro dificultades (Fácil, Media, Difícil, Experta). Cada puzzle tiene solución única, garantizada con el solver.
Validación en vivo: las celdas en conflicto se marcan según las reglas de fila, columna y caja.
Resaltado de la fila, columna y caja de la celda seleccionada, y de las demás celdas con el mismo número.
Notas (candidatos) por celda; al colocar un valor se limpian las notas de sus celdas relacionadas.
Deshacer y rehacer: cada acción del usuario es un único paso.
Pistas y opción de resolver el puzzle (la solución se muestra en una ventana aparte, sin alterar la partida).
Cronómetro con pausa.
Estadísticas por dificultad (iniciadas, ganadas, perdidas, mejor tiempo, tiempo medio) y últimas 20 partidas.
Guardar y continuar la partida en curso.
Tema claro y oscuro con paleta propia; la preferencia se guarda.
Requisitos
Windows 10 o superior
SDK de .NET 10
Compilar y ejecutar
bash
git clone https://github.com/<usuario>/<repositorio>.git
cd <repositorio>

dotnet build
dotnet run --project src/Sudoku.WinForms
Pruebas
bash
dotnet test

Las pruebas usan xUnit. Los presenters se prueban sin instanciar controles de WinForms, mediante dobles de prueba de las vistas.

Controles
Acción	Entrada
Seleccionar celda	Clic izquierdo
Mover la selección	Flechas del teclado
Colocar un valor o una nota	Teclas 1-9 (fila superior o teclado numérico)
Borrar la celda	Supr o Retroceso
Alternar modo notas	Botón «Notas» de la barra lateral

Si el modo notas está activo, los números alternan candidatos en lugar de colocar valores.

Arquitectura

Arquitectura limpia en cuatro proyectos. Las dependencias apuntan siempre hacia adentro.

Sudoku.sln
├─ src/
│  ├─ Domain                 (sin dependencias)
│  ├─ Sudoku.Application     (-> Domain)
│  ├─ Infrastructure         (-> Application, Domain)
│  └─ Sudoku.WinForms        (-> Application; Infrastructure solo en el composition root)
└─ tests/
   ├─ Domain.Tests.Unit
   ├─ Application.Tests.Unit
   ├─ Infrastructure.Tests.Unit
   └─ Sudoku.WinForms.Tests
Capa	Responsabilidad
Domain	Reglas puras del Sudoku: Board inmutable, Cell, Position, CandidateSet (bitmask) y SudokuRules como única fuente de validación.
Application	Casos de uso (uno por clase), puertos (ISolver, IPuzzleGenerator, repositorios, IClock) y modelos como GameSession.
Infrastructure	Solver con backtracking y heurística MRV, generador de puzzles, persistencia JSON con escritura atómica y reloj del sistema.
WinForms	Patrón MVP con vistas pasivas, BoardControl propio, sistema de temas y composition root con Microsoft.Extensions.DependencyInjection.

Más detalle y el registro de decisiones en ARCHITECTURE.md.

Persistencia

Los datos se guardan como JSON en %AppData%\SudokuGame\:

settings.json: tema y preferencias
statistics.json: estadísticas agregadas
games/current.json: partida en curso

Cada archivo lleva un campo version, se escribe de forma atómica (archivo temporal y reemplazo) y, si está dañado, la aplicación avisa sin modificarlo.

Stack
C# 14 / .NET 10
Windows Forms
System.Text.Json
Microsoft.Extensions.DependencyInjection
xUnit
Estado del proyecto

En desarrollo. Dominio, solver, generador, casos de uso, persistencia y la interfaz principal están implementados; queda el pulido final y ampliar las pruebas. Ver los hitos en ARCHITECTURE.md.
