namespace RtMidi.Net.Enums;

/// <summary>
/// Note inside an octave, with English (C, D...) and Latin (Do, Re...) names.
/// </summary>
public enum MusicNote : byte
{
    /// <summary>C.</summary>
    C = 0,
    /// <summary>C sharp.</summary>
    CSharp = 1,
    /// <summary>D.</summary>
    D = 2,
    /// <summary>D sharp.</summary>
    DSharp = 3,
    /// <summary>E.</summary>
    E = 4,
    /// <summary>F.</summary>
    F = 5,
    /// <summary>F sharp.</summary>
    FSharp = 6,
    /// <summary>G.</summary>
    G = 7,
    /// <summary>G sharp.</summary>
    GSharp = 8,
    /// <summary>A.</summary>
    A = 9,
    /// <summary>A sharp.</summary>
    ASharp = 10,
    /// <summary>B.</summary>
    B = 11,

    /// <summary>Do (C).</summary>
    Do = C,
    /// <summary>Do sharp (C sharp).</summary>
    DoSharp = CSharp,
    /// <summary>Re (D).</summary>
    Re = D,
    /// <summary>Re sharp (D sharp).</summary>
    ReSharp = DSharp,
    /// <summary>Mi (E).</summary>
    Mi = E,
    /// <summary>Fa (F).</summary>
    Fa = F,
    /// <summary>Fa sharp (F sharp).</summary>
    FaSharp = FSharp,
    /// <summary>Sol (G).</summary>
    Sol = G,
    /// <summary>Sol sharp (G sharp).</summary>
    SolSharp = GSharp,
    /// <summary>La (A).</summary>
    La = A,
    /// <summary>La sharp (A sharp).</summary>
    LaSharp = ASharp,
    /// <summary>Si (B).</summary>
    Si = B,
}