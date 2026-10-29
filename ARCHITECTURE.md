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
- HighlightRelatedCells: resalta la fila, columna y bloque.
- HighlightMatchingValues: resalta las demás celdas que contienen el mismo número.

## 2. Stack

- C# 14 / .NET 10
- Windows Forms (sin librerías de UI de terceros)
- Persistencia: archivos JSON (System.Text.Json), en `%AppData%\SudokuGame\`
- Pruebas: xUnit
- Inyección de dependencias: Microsoft.Extensions.DependencyInjection
- Control de versiones: git / GitHub

## 3. Estructura de la solución

```
Sudoku.sln
├─ src/
│  ├─ Sudoku.Domain          (sin dependencias)
│  ├─ Sudoku.Application     (-> Domain)
│  ├─ Sudoku.Infrastructure  (-> Application, Domain)
│  └─ Sudoku.WinForms        (-> Application, Infrastructure para el composition root)
└─ tests/
   ├─ Sudoku.Domain.Tests
   ├─ Sudoku.Application.Tests
   └─ Sudoku.Infrastructure.Tests
```

### Regla de dependencias

Las dependencias apuntan siempre hacia adentro:

- **Domain** no referencia ningún otro proyecto ni paquete.
- **Application** solo referencia Domain. Define las interfaces (puertos) que Infrastructure implementa.
- **Infrastructure** implementa esos puertos (JSON, solver, generador).
- **WinForms** solo habla con Application a través de casos de uso y presenters. Solo el composition root (`Program.cs`) conoce a Infrastructure.

## 4. Capas

### Domain

Reglas puras del Sudoku. Sin I/O, sin UI, sin serialización.

- `Position` (fila, columna) — `readonly record struct`
- `Cell` (Value?, IsGiven, Notes: CandidateSet)
- `Board` — 81 celdas, acceso por `Position`, inmutable, con Empty, FromGivens, WithValue, WithNotes, WithCleared, GivensOnly y ToValuesString
- `Difficulty` (Easy, Medium, Hard, Expert)
- `SudokuRules` — única fuente de la validación de fila / columna / caja (DRY: la usan solver, generador y UI) (Peers, IsValidPlacement, FindConflicts, IsComplete e IsSolved)
- `CandidateSet`.Union/Except
- `SudokuConstants`
- `ValueGuard`

### Application

Casos de uso y contratos.

Casos de uso (uno por clase, responsabilidad única):

- `StartNewGame`, `PlaceValue`, `ClearCell`, `ToggleNote`
- `Undo`, `Redo`
- `GetHint`, `SolveGame` (ya existe, se adelantó del hito 4 al 2)
- `SaveGame`, `LoadGame`
- `GetStatistics`, `ChangeTheme`
- PlaceValue es quien limpia los candidatos de los peers
  Puertos (interfaces):

- `ISolver` tiene Solve y CountSolutions(limit)
- `IPuzzleGenerator`
- `IGameRepository`
- `IStatisticsRepository`
- `ISettingsRepository`
- `IClock` (para poder probar tiempos)

### Infrastructure

- `BacktrackingSolver` (implementa `ISolver`)
- `PuzzleGenerator` (implementa `IPuzzleGenerator`, usa el solver para garantizar solución única)
- `JsonGameRepository`, `JsonStatisticsRepository`, `JsonSettingsRepository`
- `SystemClock`
- SolverState (internal)

### WinForms

Patrón MVP (Model-View-Presenter), con vistas pasivas:

- `MainForm` + `IMainView`
- `BoardControl` — control propio que dibuja el tablero con GDI+
- Presenters sin referencias a controles concretos (se pueden probar)
- `Program.cs` — composition root y configuración del contenedor de DI

## 5. Temas (claro y oscuro)

- `Theme` es un dato inmutable: paleta de colores con nombres semánticos (`Background`, `BoardLine`, `BoxLine`, `GivenText`, `UserText`, `SelectedCell`, `ConflictText`, `NoteText`, etc.).
- `IThemeService` expone el tema actual y un evento `ThemeChanged`.
- Los controles nunca usan colores fijos: leen del tema y se repintan al cambiar.
- La preferencia se guarda con `ISettingsRepository`.
- No dependemos del modo oscuro experimental de WinForms; una paleta propia da control total sobre el tablero.

## 6. Persistencia JSON

Carpeta: `%AppData%\SudokuGame\`

- `settings.json` — tema y preferencias
- `statistics.json` — estadísticas agregadas
- `games/current.json` — partida en curso (tablero, notas, historial, tiempo)
  Reglas:

- El esquema lleva un campo `version` para poder migrar.
- Las clases de dominio no se serializan directamente: se usan DTOs en Infrastructure.
- Escritura atómica (archivo temporal + reemplazo) para no corromper datos.

## 7. Convenciones

- Identificadores en inglés; textos de UI y documentación en español.
- Un tipo por archivo; nombres de archivo = nombre del tipo.
- `Nullable` habilitado, `ImplicitUsings` según convenga, warnings como errores en CI.
- Inmutabilidad por defecto en Domain (`record`, `readonly`).
- Sin lógica de negocio en formularios ni en repositorios.
- Cada caso de uso y regla de dominio lleva pruebas unitarias.
- SOLID y DRY: ante duplicación, extraer; ante una clase con más de una razón de cambio, dividir.
- Directory.Build.props (TFM, Nullable, ImplicitUsings)
- Los warnings como errores se activan solo con CI=true

## 8. Hitos

1. Dominio y reglas (completado)
2. Solver
3. Generador y dificultades
4. Casos de uso (colocar, notas, deshacer / rehacer, pistas)
5. Persistencia JSON (partidas, estadísticas, tema)
6. UI WinForms con temas
7. Pulido y pruebas

## 9. Decisiones registradas

| Fecha      | Decisión                                                                           |
| ---------- | ---------------------------------------------------------------------------------- |
| 2026-10-28 | Arquitectura limpia en 4 proyectos + 3 de pruebas                                  |
| 2026-10-28 | MVP en WinForms, paleta de temas propia                                            |
| 2026-10-28 | Persistencia en JSON dentro de `%AppData%`                                         |
| 2026-10-28 | Board inmutable                                                                    |
| 2026-10-28 | Notas como CandidateSet (bitmask)                                                  |
| 2026-10-28 | Excepciones para invariantes en Domain                                             |
| 2026-10-28 | Política de notas al colocar valor en el caso de uso                               |
| 2026-10-29 | Solver con backtracking y heurística MRV                                           |
| 2026-10-29 | Estado mutable interno separado de Board                                           |
| 2026-10-29 | GetHint resuelve solo con las celdas dadas y lanza excepción si no tienen solución |
| 2026-10-29 | Completado Hito 2                                                                  |
