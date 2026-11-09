namespace RtMidi.Net;

/// <summary>
/// The 128 instruments of General MIDI Level 1 (GM 1 Instrument Patch Map), as the value of a
/// <see cref="MidiMessageProgramChange"/>.
/// </summary>
/// <remarks>
/// <para>
/// The value is the Program Change data byte, 0 to 127. Most software and the GM 1 table show it as 1 to 128:
/// <see cref="AcousticGrandPiano"/> is program 1 in the table and 0 in the message. Each name is the official
/// GM 1 Sound Set name; GM 1 defines only the name of each sound, not how it sounds, and the names in parentheses
/// (synth leads, pads and effects) are only a guide.
/// </para>
/// <para>
/// A Program Change is not always General MIDI, so <see cref="MidiMessageProgramChange.Program"/> stays a byte. Cast
/// between both: <c>new MidiMessageProgramChange(channel, (byte)GeneralMidiProgram.Violin)</c> or
/// <c>(GeneralMidiProgram)message.Program</c>. Programs are grouped in families of 8, see
/// <see cref="GeneralMidi.GetFamily"/>.
/// </para>
/// </remarks>
public enum GeneralMidiProgram : byte
{
    // Piano (1-8)
    /// <summary>1. Acoustic Grand Piano.</summary>
    AcousticGrandPiano = 0,
    /// <summary>2. Bright Acoustic Piano.</summary>
    BrightAcousticPiano = 1,
    /// <summary>3. Electric Grand Piano.</summary>
    ElectricGrandPiano = 2,
    /// <summary>4. Honky-tonk Piano.</summary>
    HonkyTonkPiano = 3,
    /// <summary>5. Electric Piano 1.</summary>
    ElectricPiano1 = 4,
    /// <summary>6. Electric Piano 2.</summary>
    ElectricPiano2 = 5,
    /// <summary>7. Harpsichord.</summary>
    Harpsichord = 6,
    /// <summary>8. Clavi.</summary>
    Clavi = 7,

    // Chromatic Percussion (9-16)
    /// <summary>9. Celesta.</summary>
    Celesta = 8,
    /// <summary>10. Glockenspiel.</summary>
    Glockenspiel = 9,
    /// <summary>11. Music Box.</summary>
    MusicBox = 10,
    /// <summary>12. Vibraphone.</summary>
    Vibraphone = 11,
    /// <summary>13. Marimba.</summary>
    Marimba = 12,
    /// <summary>14. Xylophone.</summary>
    Xylophone = 13,
    /// <summary>15. Tubular Bells.</summary>
    TubularBells = 14,
    /// <summary>16. Dulcimer.</summary>
    Dulcimer = 15,

    // Organ (17-24)
    /// <summary>17. Drawbar Organ.</summary>
    DrawbarOrgan = 16,
    /// <summary>18. Percussive Organ.</summary>
    PercussiveOrgan = 17,
    /// <summary>19. Rock Organ.</summary>
    RockOrgan = 18,
    /// <summary>20. Church Organ.</summary>
    ChurchOrgan = 19,
    /// <summary>21. Reed Organ.</summary>
    ReedOrgan = 20,
    /// <summary>22. Accordion.</summary>
    Accordion = 21,
    /// <summary>23. Harmonica.</summary>
    Harmonica = 22,
    /// <summary>24. Tango Accordion.</summary>
    TangoAccordion = 23,

    // Guitar (25-32)
    /// <summary>25. Acoustic Guitar (nylon).</summary>
    AcousticGuitarNylon = 24,
    /// <summary>26. Acoustic Guitar (steel).</summary>
    AcousticGuitarSteel = 25,
    /// <summary>27. Electric Guitar (jazz).</summary>
    ElectricGuitarJazz = 26,
    /// <summary>28. Electric Guitar (clean).</summary>
    ElectricGuitarClean = 27,
    /// <summary>29. Electric Guitar (muted).</summary>
    ElectricGuitarMuted = 28,
    /// <summary>30. Overdriven Guitar.</summary>
    OverdrivenGuitar = 29,
    /// <summary>31. Distortion Guitar.</summary>
    DistortionGuitar = 30,
    /// <summary>32. Guitar harmonics.</summary>
    GuitarHarmonics = 31,

    // Bass (33-40)
    /// <summary>33. Acoustic Bass.</summary>
    AcousticBass = 32,
    /// <summary>34. Electric Bass (finger).</summary>
    ElectricBassFinger = 33,
    /// <summary>35. Electric Bass (pick).</summary>
    ElectricBassPick = 34,
    /// <summary>36. Fretless Bass.</summary>
    FretlessBass = 35,
    /// <summary>37. Slap Bass 1.</summary>
    SlapBass1 = 36,
    /// <summary>38. Slap Bass 2.</summary>
    SlapBass2 = 37,
    /// <summary>39. Synth Bass 1.</summary>
    SynthBass1 = 38,
    /// <summary>40. Synth Bass 2.</summary>
    SynthBass2 = 39,

    // Strings (41-48)
    /// <summary>41. Violin.</summary>
    Violin = 40,
    /// <summary>42. Viola.</summary>
    Viola = 41,
    /// <summary>43. Cello.</summary>
    Cello = 42,
    /// <summary>44. Contrabass.</summary>
    Contrabass = 43,
    /// <summary>45. Tremolo Strings.</summary>
    TremoloStrings = 44,
    /// <summary>46. Pizzicato Strings.</summary>
    PizzicatoStrings = 45,
    /// <summary>47. Orchestral Harp.</summary>
    OrchestralHarp = 46,
    /// <summary>48. Timpani.</summary>
    Timpani = 47,

    // Ensemble (49-56)
    /// <summary>49. String Ensemble 1.</summary>
    StringEnsemble1 = 48,
    /// <summary>50. String Ensemble 2.</summary>
    StringEnsemble2 = 49,
    /// <summary>51. SynthStrings 1.</summary>
    SynthStrings1 = 50,
    /// <summary>52. SynthStrings 2.</summary>
    SynthStrings2 = 51,
    /// <summary>53. Choir Aahs.</summary>
    ChoirAahs = 52,
    /// <summary>54. Voice Oohs.</summary>
    VoiceOohs = 53,
    /// <summary>55. Synth Voice.</summary>
    SynthVoice = 54,
    /// <summary>56. Orchestra Hit.</summary>
    OrchestraHit = 55,

    // Brass (57-64)
    /// <summary>57. Trumpet.</summary>
    Trumpet = 56,
    /// <summary>58. Trombone.</summary>
    Trombone = 57,
    /// <summary>59. Tuba.</summary>
    Tuba = 58,
    /// <summary>60. Muted Trumpet.</summary>
    MutedTrumpet = 59,
    /// <summary>61. French Horn.</summary>
    FrenchHorn = 60,
    /// <summary>62. Brass Section.</summary>
    BrassSection = 61,
    /// <summary>63. SynthBrass 1.</summary>
    SynthBrass1 = 62,
    /// <summary>64. SynthBrass 2.</summary>
    SynthBrass2 = 63,

    // Reed (65-72)
    /// <summary>65. Soprano Sax.</summary>
    SopranoSax = 64,
    /// <summary>66. Alto Sax.</summary>
    AltoSax = 65,
    /// <summary>67. Tenor Sax.</summary>
    TenorSax = 66,
    /// <summary>68. Baritone Sax.</summary>
    BaritoneSax = 67,
    /// <summary>69. Oboe.</summary>
    Oboe = 68,
    /// <summary>70. English Horn.</summary>
    EnglishHorn = 69,
    /// <summary>71. Bassoon.</summary>
    Bassoon = 70,
    /// <summary>72. Clarinet.</summary>
    Clarinet = 71,

    // Pipe (73-80)
    /// <summary>73. Piccolo.</summary>
    Piccolo = 72,
    /// <summary>74. Flute.</summary>
    Flute = 73,
    /// <summary>75. Recorder.</summary>
    Recorder = 74,
    /// <summary>76. Pan Flute.</summary>
    PanFlute = 75,
    /// <summary>77. Blown Bottle.</summary>
    BlownBottle = 76,
    /// <summary>78. Shakuhachi.</summary>
    Shakuhachi = 77,
    /// <summary>79. Whistle.</summary>
    Whistle = 78,
    /// <summary>80. Ocarina.</summary>
    Ocarina = 79,

    // Synth Lead (81-88)
    /// <summary>81. Lead 1 (square).</summary>
    Lead1Square = 80,
    /// <summary>82. Lead 2 (sawtooth).</summary>
    Lead2Sawtooth = 81,
    /// <summary>83. Lead 3 (calliope).</summary>
    Lead3Calliope = 82,
    /// <summary>84. Lead 4 (chiff).</summary>
    Lead4Chiff = 83,
    /// <summary>85. Lead 5 (charang).</summary>
    Lead5Charang = 84,
    /// <summary>86. Lead 6 (voice).</summary>
    Lead6Voice = 85,
    /// <summary>87. Lead 7 (fifths).</summary>
    Lead7Fifths = 86,
    /// <summary>88. Lead 8 (bass + lead).</summary>
    Lead8BassAndLead = 87,

    // Synth Pad (89-96)
    /// <summary>89. Pad 1 (new age).</summary>
    Pad1NewAge = 88,
    /// <summary>90. Pad 2 (warm).</summary>
    Pad2Warm = 89,
    /// <summary>91. Pad 3 (polysynth).</summary>
    Pad3Polysynth = 90,
    /// <summary>92. Pad 4 (choir).</summary>
    Pad4Choir = 91,
    /// <summary>93. Pad 5 (bowed).</summary>
    Pad5Bowed = 92,
    /// <summary>94. Pad 6 (metallic).</summary>
    Pad6Metallic = 93,
    /// <summary>95. Pad 7 (halo).</summary>
    Pad7Halo = 94,
    /// <summary>96. Pad 8 (sweep).</summary>
    Pad8Sweep = 95,

    // Synth Effects (97-104)
    /// <summary>97. FX 1 (rain).</summary>
    Fx1Rain = 96,
    /// <summary>98. FX 2 (soundtrack).</summary>
    Fx2Soundtrack = 97,
    /// <summary>99. FX 3 (crystal).</summary>
    Fx3Crystal = 98,
    /// <summary>100. FX 4 (atmosphere).</summary>
    Fx4Atmosphere = 99,
    /// <summary>101. FX 5 (brightness).</summary>
    Fx5Brightness = 100,
    /// <summary>102. FX 6 (goblins).</summary>
    Fx6Goblins = 101,
    /// <summary>103. FX 7 (echoes).</summary>
    Fx7Echoes = 102,
    /// <summary>104. FX 8 (sci-fi).</summary>
    Fx8SciFi = 103,

    // Ethnic (105-112)
    /// <summary>105. Sitar.</summary>
    Sitar = 104,
    /// <summary>106. Banjo.</summary>
    Banjo = 105,
    /// <summary>107. Shamisen.</summary>
    Shamisen = 106,
    /// <summary>108. Koto.</summary>
    Koto = 107,
    /// <summary>109. Kalimba.</summary>
    Kalimba = 108,
    /// <summary>110. Bag pipe.</summary>
    BagPipe = 109,
    /// <summary>111. Fiddle.</summary>
    Fiddle = 110,
    /// <summary>112. Shanai.</summary>
    Shanai = 111,

    // Percussive (113-120)
    /// <summary>113. Tinkle Bell.</summary>
    TinkleBell = 112,
    /// <summary>114. Agogo.</summary>
    Agogo = 113,
    /// <summary>115. Steel Drums.</summary>
    SteelDrums = 114,
    /// <summary>116. Woodblock.</summary>
    Woodblock = 115,
    /// <summary>117. Taiko Drum.</summary>
    TaikoDrum = 116,
    /// <summary>118. Melodic Tom.</summary>
    MelodicTom = 117,
    /// <summary>119. Synth Drum.</summary>
    SynthDrum = 118,
    /// <summary>120. Reverse Cymbal.</summary>
    ReverseCymbal = 119,

    // Sound Effects (121-128)
    /// <summary>121. Guitar Fret Noise.</summary>
    GuitarFretNoise = 120,
    /// <summary>122. Breath Noise.</summary>
    BreathNoise = 121,
    /// <summary>123. Seashore.</summary>
    Seashore = 122,
    /// <summary>124. Bird Tweet.</summary>
    BirdTweet = 123,
    /// <summary>125. Telephone Ring.</summary>
    TelephoneRing = 124,
    /// <summary>126. Helicopter.</summary>
    Helicopter = 125,
    /// <summary>127. Applause.</summary>
    Applause = 126,
    /// <summary>128. Gunshot.</summary>
    Gunshot = 127,
}
