using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage(
    "Major Code Smell",
    "S2325:Method can be static",
    Justification = "Métodos de UI (WPF) no deben ser estáticos por diseño.",
    Scope = "module")]
