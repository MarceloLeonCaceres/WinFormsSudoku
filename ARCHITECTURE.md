# Sudoku WinForms — Arquitectura

> Documento base del Project. Es la fuente de verdad: si una decisión cambia, se actualiza aquí.

## 1. Objetivo

Juego de Sudoku de escritorio con:

- Tablero 9x9 jugable con validación de reglas en vivo
- Generador de puzzles con niveles de dificultad
- Solver (resolver y dar pistas)
- Notas (candidatos) por celda, deshacer / rehacer
- Estadísticas (partidas jugadas, tiempos, por dificultad)
- Guardar y reanudar partidas
- Tema claro y oscuro desde el inicio, con la preferencia persistida
- HighlightRelatedCells: resalta la fila, columna y bloque. Va siempre activo.
- HighlightMatchingValues: resalta las demás celdas que contienen el mismo número. Va siempre activo.

## 2. Stack

- C# 14 / .NET 10
- Windows Forms (sin librerías de UI de terceros)
- Persistencia: archivos JSON (System.Text.Json), en `%AppData%\SudokuGame\`
- Pruebas: xUnit
- Inyección de dependencias: Microsoft.Extensions.DependencyInjection
- Control de versiones: git / GitHub

## 3. Estructura de la solución

El nombre de cada proyecto coincide con su namespace raíz.

```
Sudoku.sln
├─ src/
│  ├─ Domain                 (sin dependencias)
│  ├─ Sudoku.Application     (-> Domain)
│  ├─ Infrastructure         (-> Application, Domain)
│  └─ Sudoku.WinForms        (-> Application, Infrastructure solo para el composition root)
└─ tests/
   ├─ Domain.Tests.Unit
   ├─ Application.Tests.Unit
   ├─ Infrastructure.Tests.Unit
   └─ Sudoku.WinForms.Tests
```

### Regla de dependencias

Las dependencias apuntan siempre hacia adentro:

- **Domain** no referencia ningún otro proyecto ni paquete.
- **Application** solo referencia Domain. Define las interfaces (puertos) que Infrastructure implementa.
- **Infrastructure** implementa esos puertos (JSON, solver, generador, reloj).
- **WinForms** solo habla con Application a través de casos de uso y presenters. Solo el composition root (`Program.cs` / `CompositionRoot`) conoce a Infrastructure.

## 4. Capas

### Domain

Reglas puras del Sudoku. Sin I/O, sin UI, sin serialización.

- `Position` (fila, columna) — `readonly record struct`; expone `Box`, `Index`, `All` y `FromIndex`
- `Cell` (Value?, IsGiven, Notes: CandidateSet) — `readonly record struct`
- `Board` — 81 celdas, acceso por `Position`, inmutable, con `Empty`, `FromGivens`, `WithValue`, `WithNotes`, `WithCleared`, `GivensOnly` y `ToValuesString`
- `Difficulty` (Easy, Medium, Hard, Expert)
- `SudokuRules` — única fuente de la validación de fila / columna / caja (DRY: la usan solver, generador y UI): `Peers`, `IsValidPlacement`, `FindConflicts`, `IsComplete` e `IsSolved`
- `CandidateSet` — bitmask inmutable (`Of`, `Contains`, `Add`, `Remove`, `Toggle`, `Union`, `Except`)
- `SudokuConstants`
- `ValueGuard`

### Application

Casos de uso y contratos.

Casos de uso (uno por clase, responsabilidad única):

- `StartNewGame` (genera el puzzle y devuelve una `GameSession`)
- `PlaceValue` (también limpia los candidatos de los peers), `ClearCell`, `ToggleNote`
- `Undo`, `Redo`
- `GetHint` (devuelve un `Hint`), `ApplyHint` (usa `GetHint` + `PlaceValue`)
- `SaveGame`, `LoadGame`
- `RecordGameStarted`, `RecordGameFinished`
- `GetStatistics`, `GetSettings`, `ChangeTheme`
- Pendiente de implementar: `SolveGame` (lo necesita `IMainView.SolveRequested`)

Puertos (interfaces):

- `ISolver` — `Solve` y `CountSolutions(limit)`
- `IPuzzleGenerator` — `Generate(Difficulty, CancellationToken)` devuelve un `Puzzle` de solución única
- `IGameRepository`
- `IStatisticsRepository`
- `ISettingsRepository`
- `IClock` (para poder probar tiempos)

Modelos y políticas:

- `Puzzle` (Board, Solution, Difficulty) — resultado del generador
- `DifficultyProfile` — rango de celdas dadas por dificultad (única fuente de estos rangos)
- `Hint` (Position, Value)
- `GameSession` — tablero, historial y tiempo
- `SavedGame`
- `PlayTimer`
- `CorruptSaveException`
- `GameStatistics`, `DifficultyStatistics`, `GameRecord`, `GameOutcome`
- `AppSettings`
- `ThemeKind` — la paleta (`Theme`) es de WinForms

### Infrastructure

- `BacktrackingSolver` (implementa `ISolver`)
- `PuzzleGenerator` (implementa `IPuzzleGenerator`; `Random` inyectable; usa el solver para garantizar solución única)
- `JsonGameRepository`, `JsonStatisticsRepository`, `JsonSettingsRepository`
- `SystemClock`
- `JsonFileStore`, `AtomicFile`
- `IAppDataPaths` / `AppDataPaths`
- Internos: `SolverState`, `GameDto` / `GameMapper`, `StatisticsDto` / `StatisticsMapper`, `DifficultyStatisticsDto`, `GameRecordDto`, `SettingsDto` / `SettingsMapper`, `SchemaVersion`, `EnumParser`, `CorruptSaveGuard`

### WinForms

Patrón MVP (Model-View-Presenter), con vistas pasivas:

- `Views/MainForm` + `Views/IMainView`: ventana principal y contrato de su vista. IMainView no expone controles WinForms; compone IBoardView (tablero) e IGameControlsView (barra lateral y controles de juego, incluida la pausa). MainForm recibe IThemeService y rellena los combos de dificultad y tema con Choice<T>.
- `Controls/BoardControl/BoardControl`: control personalizado que dibuja el tablero Sudoku mediante GDI+.
- `Controls/BoardControl/BoardGeometry`: calcula la geometría del tablero y realiza el hit testing. No depende de WinForms ni de GDI+.
- `Controls/BoardControl/BoardRenderer`: responsable exclusivamente del dibujo del tablero. Es interno y estático; recibe el estado del dominio y el tema actual.
- `SelectionChangedEventArgs` y `ValueEnteredEventArgs`: argumentos de los eventos del control.
- Presenters sin referencias a controles concretos. Coordinan las vistas y los casos de uso de Application.
- `Program.cs`: composition root y configuración del contenedor de DI.
- `CompositionRoot`: centraliza el registro de dependencias.
- `Theming`: contiene `Theme`, `Palettes`, `IThemeService`, `ThemeService` y `ThemeApplier`.
- `Views/IBoardView`, `Presenters/ (MainPresenter, BoardPresenter, CurrentGame)`, `Views/IGameControlsView` y `Services/ (IDialogService, MessageBoxDialogService)`; IMainView compone IBoardView + IGameControlsView».
- `MainPresenter`: nueva partida (generación en segundo plano, cancelable desde la vista con CancelRequested) y pausa (alterna session.Pause()/Start() y avisa con SetPaused). Se ignora la pausa si no hay sesión o se está generando.

#### BoardControl

`BoardControl` es responsable de la presentación e interacción directa con el tablero:

- Recibe el tablero mediante `SetBoard(Board)` (no admite `null`). No mantiene una copia mutable del modelo de dominio.
- Utiliza `Domain.Board`, `Domain.Position`, `Domain.Cell` y `Domain.SudokuRules`. No se crean modelos de dominio duplicados en WinForms.
- Expone `SelectedPosition` (`Position?`, con setter público) y `NotesMode`. Asignar `SelectedPosition` a una celda distinta dispara `SelectionChanged`.
- Expone los eventos `SelectionChanged`, `ValueEntered` y `ClearRequested`.
- La selección se realiza con el botón izquierdo del ratón (el clic da el foco al control) y se puede mover mediante las flechas del teclado.
- Las teclas 1-9 (fila superior y teclado numérico) generan `ValueEntered`; el control no decide si se coloca un valor o una nota. Esa decisión corresponde al Presenter.
- Delete y Backspace generan `ClearRequested`.
- Los eventos de teclado solo se emiten si hay una celda seleccionada.
- La selección puede ser `null`. Seleccionar otra celda genera `SelectionChanged`; deseleccionar no genera ese evento.
- Un clic fuera del tablero conserva la selección actual. La navegación con flechas se detiene en los bordes.
- El tablero siempre se dibuja cuadrado y centrado dentro del espacio disponible.
- Las notas se dibujan en una cuadrícula fija 3x3.
- El constructor exige `IThemeService`, por lo que el control no se puede instanciar desde el diseñador: se crea por código.
- El control no contiene lógica de negocio ni accede a `GameSession`.

#### Dibujo y geometría

- `BoardGeometry` calcula el área cuadrada, el tamaño de las celdas y la posición correspondiente a un punto del ratón. El hit testing usa intervalos semiabiertos: el borde derecho e inferior del tablero devuelven `null`.
- `BoardRenderer` dibuja el fondo, las celdas, los valores, las notas, los resaltados y las líneas del tablero.
- Doble búfer: `UserPaint`, `AllPaintingInWmPaint`, `OptimizedDoubleBuffer` y `ResizeRedraw`; `OnPaintBackground` queda vacío porque el renderer pinta toda el área.
- Todo se calcula en píxeles a partir de `ClientSize`, así que el dibujo no depende de la escala DPI. Las fuentes usan `GraphicsUnit.Pixel` proporcional a `CellSize`: valores y dadas 0,50 (las dadas en negrita) y notas 0,20. Los grosores de línea también escalan con `CellSize` (con mínimo y máximo).
- El texto se dibuja con `TextRenderer` (GDI), centrado en la celda o en el hueco 3x3 de la nota.
- `Theme.Surface` rellena el área fuera del tablero y `Theme.BoardBackground` el tablero.
- El resaltado de fila, columna y caja (`HighlightRelatedCells`, mediante `SudokuRules.Peers`) y el de valores coincidentes (`HighlightMatchingValues`) siempre están activos.
- Los conflictos se calculan mediante `SudokuRules.FindConflicts`.
- Los colores se obtienen exclusivamente de `IThemeService.Current`. No se utilizan colores fijos.
- El control escucha `ThemeChanged` (y se desuscribe en `Dispose`) y solicita un repintado cuando cambia el tema.

#### Pruebas WinForms

- `BoardGeometryTests`: pruebas unitarias de dimensiones, centrado, límites de celdas y hit testing.
- La lógica de los Presenters se prueba sin instanciar controles WinForms.
- La lógica de Domain y Application continúa probándose en sus respectivos proyectos de pruebas.

## 5. Temas (claro y oscuro)

- `Theme` es un dato inmutable: paleta de colores con nombres semánticos (`Background`, `Surface`, `Text`, `MutedText`, `Accent`, `Control`, `ControlText`, `ControlBorder`, `BoardBackground`, `BoardLine`, `BoxLine`, `GivenText`, `UserText`, `SelectedCell`, `RelatedCell`, `MatchingValue`, `ConflictText`, `NoteText`).
- `IThemeService` expone el tema actual (`Current`), un evento `ThemeChanged` y `SetTheme(ThemeKind)`, que persiste primero y cambia después. Si guardar falla, la excepción se propaga y el tema no cambia. Un tema guardado dañado en el arranque avisa y usa el defecto sin tocar el archivo.
- Los controles nunca usan colores fijos: leen del tema y se repintan al cambiar. `ThemeApplier` pinta los controles estándar; los propios (como `BoardControl`) leen el tema directamente.
- La preferencia se guarda con `ISettingsRepository`.
- No dependemos del modo oscuro experimental de WinForms; una paleta propia da control total sobre el tablero.

## 6. Persistencia JSON

Carpeta: `%AppData%\SudokuGame\`

- `settings.json` — tema y preferencias
- `statistics.json` — estadísticas agregadas
- `games/current.json` — partida en curso (dificultad, tablero con dadas, valores y notas, y tiempo; sin historial)

Reglas:

- El esquema lleva un campo `version` para poder migrar.
- Las clases de dominio no se serializan directamente: se usan DTOs en Infrastructure.
- Los tres archivos se leen y escriben con `JsonFileStore`, con escritura atómica (archivo temporal + reemplazo) para no corromper datos.
- Un archivo ilegible, dañado o de versión desconocida lanza `CorruptSaveException` y no se modifica.

## 7. Convenciones

- Identificadores en inglés; textos de UI y documentación en español.
- Un tipo por archivo; nombres de archivo = nombre del tipo.
- `Nullable` habilitado, `ImplicitUsings` según convenga, warnings como errores en CI.
- Inmutabilidad por defecto en Domain (`record`, `readonly`).
- Sin lógica de negocio en formularios ni en repositorios.
- Cada caso de uso y regla de dominio lleva pruebas unitarias.
- SOLID y DRY: ante duplicación, extraer; ante una clase con más de una razón de cambio, dividir.
- `Directory.Build.props` (TFM, Nullable, ImplicitUsings).
- Los warnings como errores se activan solo con `CI=true`.

## 8. Hitos

1. Dominio y reglas (completado)
2. Solver (completado)
3. Generador y dificultades (completado)
4. Casos de uso (colocar, notas, deshacer / rehacer, pistas) (completado)
5. Persistencia JSON (completado)
6. UI WinForms con temas:
   - Parte 1: infraestructura de temas y composición de dependencias (completada).
   - Parte 2: BoardControl con dibujo GDI+ y geometría (completada).
   - Parte 3: integración del tablero con MVP y casos de uso (completada).
   - Parte 4: ventana principal, barra lateral y controles de juego (completada).
   - Parte 5: interacción completa, temporizador que refresca el tiempo, deshacer/rehacer, pista, resolver (SolveGame), guardado/reanudación, estadísticas, cambio de tema, y registrar como perdida la partida que se reemplaza o abandona (pendiente: un chat para esta parte).
7. Pulido y pruebas.

## 9. Decisiones registradas

| Fecha | Decisión |
| --- | --- |
| 2026-10-28 | Arquitectura limpia en 4 proyectos + 4 de pruebas |
| 2026-10-28 | MVP en WinForms, paleta de temas propia |
| 2026-10-28 | Persistencia en JSON dentro de `%AppData%` |
| 2026-10-28 | Board inmutable |
| 2026-10-28 | Notas como CandidateSet (bitmask) |
| 2026-10-28 | Excepciones para invariantes en Domain |
| 2026-10-28 | Política de notas al colocar valor en el caso de uso |
| 2026-10-29 | Solver con backtracking y heurística MRV |
| 2026-10-29 | Estado mutable interno separado de Board |
| 2026-10-29 | GetHint resuelve solo con las celdas dadas y lanza excepción si no tienen solución |
| 2026-10-29 | Completado Hito 2 |
| 2026-10-29 | Dificultad por rango de celdas dadas (`DifficultyProfile` en Application); técnicas → hito 7 |
| 2026-10-29 | `IPuzzleGenerator.Generate(Difficulty, CancellationToken)` devuelve `Puzzle(Board, Solution, Difficulty)` |
| 2026-10-29 | Generador: solución sembrada con cajas diagonales + quitar celdas con unicidad (`CountSolutions(…, 2)`) |
| 2026-10-29 | `Random` inyectable en `PuzzleGenerator`, sin puerto nuevo; sin simetría |
| 2026-10-29 | Completado Hito 3 |
| 2026-10-29 | GameSession (Application): tablero actual + historial de snapshots de Board |
| 2026-10-29 | Casos de uso de edición devuelven bool; celdas dadas se ignoran sin excepción |
| 2026-10-29 | PlaceValue permite valores en conflicto; la UI los marca con FindConflicts |
| 2026-10-29 | Cada acción del usuario es un único paso de deshacer |
| 2026-10-29 | ApplyHint compone GetHint + PlaceValue; StartNewGame sin tiempo ni IClock |
| 2026-10-29 | Namespaces de Application unificados en Sudoku.Application.\*; Puzzles.cs → Puzzle.cs |
| 2026-10-29 | Completado Hito 4 |
| 2026-10-29 | GameSession lleva tiempo con IClock y PlayTimer; revierte “StartNewGame sin tiempo ni IClock” |
| 2026-10-29 | current.json no guarda historial |
| 2026-10-29 | Guardado dañado: CorruptSaveException, archivo intacto |
| 2026-10-29 | JsonFileStore/AtomicFile compartidos y IAppDataPaths como fuente de rutas |
| 2026-10-29 | Sesiones nuevas y reanudadas nacen en pausa |
| 2026-10-29 | Estadísticas: agregados por dificultad + últimas 20 partidas |
| 2026-10-29 | Partida cuenta al empezar; abandonar = perdida |
| 2026-10-29 | Tema: solo Claro/Oscuro (ThemeKind), defecto Oscuro |
| 2026-10-29 | Victoria solo se registra con tablero resuelto |
| 2026-10-29 | Ajustes y estadísticas dañados lanzan CorruptSaveException |
| 2026-10-29 | Completado Hito 5 |
| 2026-10-30 | Resaltados siempre activos (no están en AppSettings) |
| 2026-10-30 | Una ventana con barra lateral y estadísticas en un diálogo |
| 2026-10-30 | Generación con Task.Run y cancelación |
| 2026-10-30 | Solo teclado y ratón |
| 2026-10-30 | BoardControl utiliza el Board inmutable de Domain; no existe un modelo de tablero duplicado en WinForms |
| 2026-10-30 | BoardControl recibe actualizaciones mediante SetBoard(Board), no mediante una propiedad pública |
| 2026-10-30 | BoardControl utiliza BoardGeometry para geometría e hit testing, y BoardRenderer para dibujo GDI+ |
| 2026-10-30 | El tablero es cuadrado y centrado; tipografía Segoe UI y notas en posiciones fijas 3x3 |
| 2026-10-30 | SelectionChanged solo se dispara al seleccionar otra celda; SelectedPosition admite null |
| 2026-10-30 | La navegación con flechas se detiene en los bordes; un clic fuera del tablero conserva la selección |
| 2026-10-30 | BoardControl emite ValueEntered independientemente de NotesMode; el Presenter decide entre colocar valor y alternar nota |
| 2026-10-30 | Las propiedades de estado del control no se serializan en el diseñador WinForms |
| 2026-10-30 | BoardControl exige IThemeService en el constructor: se crea por código, no desde el diseñador |
| 2026-10-30 | Dibujo en píxeles proporcional a CellSize (fuentes: valores y dadas 0,50, notas 0,20; texto con TextRenderer); sin dependencia del DPI |
| 2026-10-30 | Doble búfer con OptimizedDoubleBuffer + AllPaintingInWmPaint + UserPaint + ResizeRedraw; OnPaintBackground vacío |
| 2026-10-30 | HitTest usa intervalos semiabiertos: el borde derecho e inferior del tablero devuelven null |
| 2026-10-30 | Se aceptan las teclas 1-9 de la fila superior y del teclado numérico; solo el botón izquierdo del ratón selecciona |
| 2026-10-30 | SelectedPosition tiene setter público y dispara SelectionChanged al asignar una celda distinta |
| 2026-10-30 | Vistas en `Sudoku.WinForms.Views` (`IMainView`, `MainForm`) y controles en `Sudoku.WinForms.Controls.BoardControl`; `SelectionChangedEventArgs` y `ValueEnteredEventArgs` propios |
| 2026-10-30 | MainForm recibe IThemeService y usa `Choice<T>` para los combos de dificultad y tema |
| 2026-10-30 | Hito 6, partes 3, 4, y 5: un chat por parte, en ese orden |
| 2026-10-30 | En pausa solo se detiene el cronómetro y se bloquea la entrada; el tablero sigue visible |
| 2026-10-30 | SolveGame muestra la solución en una vista aparte y no modifica la GameSession ni las estadísticas |
| 2026-10-31 | `IBoardView` se separa de `IMainView`, que ahora hereda de ella (ISP). |
| 2026-10-31 | Los errores de UI pasan por el puerto `IDialogService` (implementado por `MessageBoxDialogService`). |
| 2026-10-31 | `BoardPresenter` ignora la entrada si no hay sesión, no hay selección o `!session.IsRunning`. |
| 2026-10-31 | `MainPresenter` llama a `session.Start()` tras generar y avisa a la vista con `SetPaused(false)`; sin ese aviso `MainForm` dejaba el tablero deshabilitado (`UpdateInteractionState`).|
| 2026-10-31 | `StartNewGameAsync()` es pública, no lanza excepciones, y una petición nueva cancela la generación anterior. |
| 2026-10-31 | `CompositionRoot`: `MainForm` queda como un único singleton, con `IMainView` e `IBoardView` resueltos desde la misma instancia.  |
| 2026-10-31 | IMainView se divide: compone IBoardView + IGameControlsView (ISP). MainPresenter sigue dependiendo de IMainView. |
| 2026-10-31 | Cancelar la generación: el botón «Nueva partida» pasa a «Cancelar» mientras la vista está ocupada (evento CancelRequested). Cancelar no muestra diálogo de error; el estado queda en «Generación cancelada». |
| 2026-10-31 | Solo la generación vigente llama a SetBusy(false): una generación reemplazada no apaga el «ocupado» de la nueva. |
| 2026-10-31 | La pausa vive en MainPresenter, que llama directamente a session.Pause()/Start() (sin caso de uso). Se ignora sin sesión o mientras se genera. |
| 2026-10-31 | Tras generar, MainPresenter muestra en la vista la dificultad de la sesión (SetDifficulty). |