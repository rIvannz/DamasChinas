# Universidad Veracruzana  
**Lic. en Ingeniería de Software**  
**Facultad de Estadística e Informática**  
**Tecnologías para la Construcción de Software**

---

## Estándar de Codificación para el juego Damas Chinas

**Realizado por:**  
Rodrigo Iván Ahumada Rodríguez (S21013886)  
Marquez Rodríguez Seth (S23014042)

**Docente:**  
MCC. Juan Carlos Pérez Arriaga  

**Fecha de entrega:**  
Xalapa, Ver., 2 de Octubre de 2025

---

## Tabla de Contenidos

1. [Introducción](#introducción)  
2. [Propósito](#propósito)  
3. [Idioma](#idioma)  
4. [Reglas de nombramiento](#reglas-de-nombramiento)  
   - [Clases](#clases)  
   - [Interfaces](#interfaces)  
   - [Namespaces](#namespaces)  
   - [Variables](#variables)  
   - [Constantes](#constantes)  
   - [Métodos](#métodos)  
   - [Getters y Setters](#getters-y-setters)  
5. [Estilo de código](#estilo-de-código)  
6. [Comentarios](#comentarios)  
7. [Estructuras de control](#estructuras-de-control)  
8. [Nomenclatura en WPF](#nomenclatura-en-wpf)  
9. [Manejo de excepciones](#manejo-de-excepciones)  
10. [Bitácora](#bitácora)  
11. [Pruebas](#pruebas)  
12. [Seguridad](#seguridad)  
13. [Declaración de uso de IA](#declaración-de-uso-de-ia)  
14. [Referencias](#referencias)

---

## Introducción

El propósito de un estándar de codificación es mantener un código fuente **legible, consistente y mantenible**. Este documento define las reglas para el proyecto del videojuego *Damas Chinas* desarrollado en **C#**, tomando como base las convenciones oficiales de Microsoft.

---

## Propósito

Proporcionar una guía completa y coherente para la escritura de código en C#, asegurando calidad, legibilidad y colaboración efectiva entre los miembros del equipo.

---

## Idioma

El código fuente debe escribirse en **inglés**, idioma estándar en la industria del software, para garantizar comprensión y colaboración internacional.

---

## Reglas de nombramiento

Las reglas de nombramiento garantizan coherencia, legibilidad y organización del código.

### Clases
- Se nombran en **PascalCase**.  
- Deben ser claras y descriptivas.  
- Los campos deben ser **privados** con propiedades públicas.  
- Métodos y constructores usan también PascalCase.  

### Interfaces
- Se escriben en inglés.  
- Se usa **PascalCase** con prefijo `I`.  
  Ejemplo: `IChatService`.

### Namespaces
- Usar **PascalCase** con estructura jerárquica (`Proyecto.Modulo.Submodulo`).  
- Evitar abreviaturas innecesarias.  
- Incluir el prefijo del proyecto para evitar conflictos.

### Variables
- **camelCase** para variables locales y parámetros.  
- **_camelCase** para campos privados.  
- **PascalCase** para propiedades públicas.  
- Evitar abreviaturas y nombres ambiguos.  
- No declarar más de una variable por línea.

### Constantes
- Usar **PascalCase**.  
- Pueden incluir prefijos como `Max`, `Min`, `Default`.  
- Declararse con `const`.

### Métodos
- Nombrar en **PascalCase** usando verbos o frases verbales.  
- Los parámetros usan **camelCase**.

### Getters y Setters
- Preferir propiedades en lugar de campos públicos.  
- Propiedades públicas en **PascalCase**.  
- Campos de respaldo en **_camelCase**.

---

## Estilo de código

### Indentación
- Usar **tabuladores** (no espacios).  
- Cada nuevo bloque agrega un nivel de tabulación.  
- No mezclar tabs y espacios.

### Líneas y espacios en blanco
- Una instrucción por línea.  
- Línea en blanco entre métodos.  
- Espacios alrededor de operadores y después de palabras clave (`if`, `for`, etc.).  
- Espacio después de comas entre parámetros.

### Uso de llaves
- Usar estilo **Allman** (llaves en nueva línea).  
- Siempre usar llaves `{}` incluso para una sola línea.

---

## Comentarios

Los comentarios deben aportar valor y claridad al código.  
Se permiten en español o inglés durante desarrollo.

### Comentarios de línea única
Usar `//` antes del código que describen, nunca al final de la línea.

### Comentarios de bloque
Usar `/* ... */` solo para desactivar temporalmente código o explicar decisiones no evidentes.

### Comentarios de documentación
Usar `///` para generar documentación XML:

```xml
/// <summary>Describe el método.</summary>
/// <param name="x">Descripción del parámetro.</param>
/// <returns>Valor devuelto.</returns>
/// <exception>Tipo de excepción.</exception>
```

---

## Estructuras de control

Usar llaves **Allman**, indentación con tabuladores y espacios después de palabras clave.

Ejemplo:

```csharp
if (condition)
{
	DoSomething();
}
else
{
	DoSomethingElse();
}
```

---

## Nomenclatura en WPF

Los nombres de controles deben usar **camelCase con prefijo**:
- Ejemplo: `txtEmail`, `btnSave`, `lblMessage`.

**Reglas adicionales:**
- Un atributo por línea.  
- Comentarios visuales en bloque:  
  `<!-- ===== Sección ===== -->`  
- Indentación por tabuladores.  
- Recursos organizados por tipo (`Assets/Icons`, `Assets/Fonts`, etc.).

---

## Manejo de excepciones

- Solo usar excepciones para casos **excepcionales**.  
- Capturar tipos específicos, no genéricos.  
- No usar bloques `catch` vacíos.  
- Usar `finally` para liberar recursos.  
- Variables de excepción: `e` o `ex`.

---

## Bitácora

Registrar eventos del sistema para diagnóstico y trazabilidad.

### Fatal
Errores críticos que detienen la aplicación.  
Ejemplo: pérdida de conexión al servidor de autenticación.

### Error
Errores no fatales que afectan una funcionalidad.  
Ejemplo: error al guardar configuración del perfil.

### Warning
Advertencias que no interrumpen la ejecución.  
Ejemplo: latencia alta de red.

### Trace / Información
Eventos informativos.  
Ejemplo:  
> “El jugador ‘rodrigo.ahumada’ ha iniciado una partida.”

---

## Pruebas

### Convenciones
- Clases: `TestGameLogic` o `GameLogicTest`.  
- Métodos: `TestMovePiece_WhenPositionIsValid_ShouldReturnTrue`.  

### Patrón AAA
1. **Arrange:** Preparar entorno.  
2. **Act:** Ejecutar acción.  
3. **Assert:** Verificar resultado.

---

## Seguridad

- Usar **consultas parametrizadas** para evitar inyección SQL.  
- Almacenar datos sensibles con **hash**.  
- **Validar entradas** antes de procesarlas.  
- Nombres de pruebas unitarias deben ser claros y descriptivos.



## Referencias

- BillWagner. *.NET Coding Conventions - C#.* [Microsoft Learn](https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions)  
- AMollis. *Coding Guidelines.* [Microsoft Learn](https://learn.microsoft.com/en-us/previous-versions/mixed-reality/world-locking-tools/Documentation/HowTos/CodingConventions)
