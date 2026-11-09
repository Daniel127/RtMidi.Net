namespace RtMidi.Net.Tests;

public class GeneralMidiTests
{
    // Every program and percussion note of the GM 1 Sound Set (midi.org), as wire values:
    // the table shows programs as 1-128, the Program Change byte is 0-127.
    [Theory]
    [InlineData(GeneralMidiProgram.AcousticGrandPiano, 0)]
    [InlineData(GeneralMidiProgram.BrightAcousticPiano, 1)]
    [InlineData(GeneralMidiProgram.ElectricGrandPiano, 2)]
    [InlineData(GeneralMidiProgram.HonkyTonkPiano, 3)]
    [InlineData(GeneralMidiProgram.ElectricPiano1, 4)]
    [InlineData(GeneralMidiProgram.ElectricPiano2, 5)]
    [InlineData(GeneralMidiProgram.Harpsichord, 6)]
    [InlineData(GeneralMidiProgram.Clavi, 7)]
    [InlineData(GeneralMidiProgram.Celesta, 8)]
    [InlineData(GeneralMidiProgram.Glockenspiel, 9)]
    [InlineData(GeneralMidiProgram.MusicBox, 10)]
    [InlineData(GeneralMidiProgram.Vibraphone, 11)]
    [InlineData(GeneralMidiProgram.Marimba, 12)]
    [InlineData(GeneralMidiProgram.Xylophone, 13)]
    [InlineData(GeneralMidiProgram.TubularBells, 14)]
    [InlineData(GeneralMidiProgram.Dulcimer, 15)]
    [InlineData(GeneralMidiProgram.DrawbarOrgan, 16)]
    [InlineData(GeneralMidiProgram.PercussiveOrgan, 17)]
    [InlineData(GeneralMidiProgram.RockOrgan, 18)]
    [InlineData(GeneralMidiProgram.ChurchOrgan, 19)]
    [InlineData(GeneralMidiProgram.ReedOrgan, 20)]
    [InlineData(GeneralMidiProgram.Accordion, 21)]
    [InlineData(GeneralMidiProgram.Harmonica, 22)]
    [InlineData(GeneralMidiProgram.TangoAccordion, 23)]
    [InlineData(GeneralMidiProgram.AcousticGuitarNylon, 24)]
    [InlineData(GeneralMidiProgram.AcousticGuitarSteel, 25)]
    [InlineData(GeneralMidiProgram.ElectricGuitarJazz, 26)]
    [InlineData(GeneralMidiProgram.ElectricGuitarClean, 27)]
    [InlineData(GeneralMidiProgram.ElectricGuitarMuted, 28)]
    [InlineData(GeneralMidiProgram.OverdrivenGuitar, 29)]
    [InlineData(GeneralMidiProgram.DistortionGuitar, 30)]
    [InlineData(GeneralMidiProgram.GuitarHarmonics, 31)]
    [InlineData(GeneralMidiProgram.AcousticBass, 32)]
    [InlineData(GeneralMidiProgram.ElectricBassFinger, 33)]
    [InlineData(GeneralMidiProgram.ElectricBassPick, 34)]
    [InlineData(GeneralMidiProgram.FretlessBass, 35)]
    [InlineData(GeneralMidiProgram.SlapBass1, 36)]
    [InlineData(GeneralMidiProgram.SlapBass2, 37)]
    [InlineData(GeneralMidiProgram.SynthBass1, 38)]
    [InlineData(GeneralMidiProgram.SynthBass2, 39)]
    [InlineData(GeneralMidiProgram.Violin, 40)]
    [InlineData(GeneralMidiProgram.Viola, 41)]
    [InlineData(GeneralMidiProgram.Cello, 42)]
    [InlineData(GeneralMidiProgram.Contrabass, 43)]
    [InlineData(GeneralMidiProgram.TremoloStrings, 44)]
    [InlineData(GeneralMidiProgram.PizzicatoStrings, 45)]
    [InlineData(GeneralMidiProgram.OrchestralHarp, 46)]
    [InlineData(GeneralMidiProgram.Timpani, 47)]
    [InlineData(GeneralMidiProgram.StringEnsemble1, 48)]
    [InlineData(GeneralMidiProgram.StringEnsemble2, 49)]
    [InlineData(GeneralMidiProgram.SynthStrings1, 50)]
    [InlineData(GeneralMidiProgram.SynthStrings2, 51)]
    [InlineData(GeneralMidiProgram.ChoirAahs, 52)]
    [InlineData(GeneralMidiProgram.VoiceOohs, 53)]
    [InlineData(GeneralMidiProgram.SynthVoice, 54)]
    [InlineData(GeneralMidiProgram.OrchestraHit, 55)]
    [InlineData(GeneralMidiProgram.Trumpet, 56)]
    [InlineData(GeneralMidiProgram.Trombone, 57)]
    [InlineData(GeneralMidiProgram.Tuba, 58)]
    [InlineData(GeneralMidiProgram.MutedTrumpet, 59)]
    [InlineData(GeneralMidiProgram.FrenchHorn, 60)]
    [InlineData(GeneralMidiProgram.BrassSection, 61)]
    [InlineData(GeneralMidiProgram.SynthBrass1, 62)]
    [InlineData(GeneralMidiProgram.SynthBrass2, 63)]
    [InlineData(GeneralMidiProgram.SopranoSax, 64)]
    [InlineData(GeneralMidiProgram.AltoSax, 65)]
    [InlineData(GeneralMidiProgram.TenorSax, 66)]
    [InlineData(GeneralMidiProgram.BaritoneSax, 67)]
    [InlineData(GeneralMidiProgram.Oboe, 68)]
    [InlineData(GeneralMidiProgram.EnglishHorn, 69)]
    [InlineData(GeneralMidiProgram.Bassoon, 70)]
    [InlineData(GeneralMidiProgram.Clarinet, 71)]
    [InlineData(GeneralMidiProgram.Piccolo, 72)]
    [InlineData(GeneralMidiProgram.Flute, 73)]
    [InlineData(GeneralMidiProgram.Recorder, 74)]
    [InlineData(GeneralMidiProgram.PanFlute, 75)]
    [InlineData(GeneralMidiProgram.BlownBottle, 76)]
    [InlineData(GeneralMidiProgram.Shakuhachi, 77)]
    [InlineData(GeneralMidiProgram.Whistle, 78)]
    [InlineData(GeneralMidiProgram.Ocarina, 79)]
    [InlineData(GeneralMidiProgram.Lead1Square, 80)]
    [InlineData(GeneralMidiProgram.Lead2Sawtooth, 81)]
    [InlineData(GeneralMidiProgram.Lead3Calliope, 82)]
    [InlineData(GeneralMidiProgram.Lead4Chiff, 83)]
    [InlineData(GeneralMidiProgram.Lead5Charang, 84)]
    [InlineData(GeneralMidiProgram.Lead6Voice, 85)]
    [InlineData(GeneralMidiProgram.Lead7Fifths, 86)]
    [InlineData(GeneralMidiProgram.Lead8BassAndLead, 87)]
    [InlineData(GeneralMidiProgram.Pad1NewAge, 88)]
    [InlineData(GeneralMidiProgram.Pad2Warm, 89)]
    [InlineData(GeneralMidiProgram.Pad3Polysynth, 90)]
    [InlineData(GeneralMidiProgram.Pad4Choir, 91)]
    [InlineData(GeneralMidiProgram.Pad5Bowed, 92)]
    [InlineData(GeneralMidiProgram.Pad6Metallic, 93)]
    [InlineData(GeneralMidiProgram.Pad7Halo, 94)]
    [InlineData(GeneralMidiProgram.Pad8Sweep, 95)]
    [InlineData(GeneralMidiProgram.Fx1Rain, 96)]
    [InlineData(GeneralMidiProgram.Fx2Soundtrack, 97)]
    [InlineData(GeneralMidiProgram.Fx3Crystal, 98)]
    [InlineData(GeneralMidiProgram.Fx4Atmosphere, 99)]
    [InlineData(GeneralMidiProgram.Fx5Brightness, 100)]
    [InlineData(GeneralMidiProgram.Fx6Goblins, 101)]
    [InlineData(GeneralMidiProgram.Fx7Echoes, 102)]
    [InlineData(GeneralMidiProgram.Fx8SciFi, 103)]
    [InlineData(GeneralMidiProgram.Sitar, 104)]
    [InlineData(GeneralMidiProgram.Banjo, 105)]
    [InlineData(GeneralMidiProgram.Shamisen, 106)]
    [InlineData(GeneralMidiProgram.Koto, 107)]
    [InlineData(GeneralMidiProgram.Kalimba, 108)]
    [InlineData(GeneralMidiProgram.BagPipe, 109)]
    [InlineData(GeneralMidiProgram.Fiddle, 110)]
    [InlineData(GeneralMidiProgram.Shanai, 111)]
    [InlineData(GeneralMidiProgram.TinkleBell, 112)]
    [InlineData(GeneralMidiProgram.Agogo, 113)]
    [InlineData(GeneralMidiProgram.SteelDrums, 114)]
    [InlineData(GeneralMidiProgram.Woodblock, 115)]
    [InlineData(GeneralMidiProgram.TaikoDrum, 116)]
    [InlineData(GeneralMidiProgram.MelodicTom, 117)]
    [InlineData(GeneralMidiProgram.SynthDrum, 118)]
    [InlineData(GeneralMidiProgram.ReverseCymbal, 119)]
    [InlineData(GeneralMidiProgram.GuitarFretNoise, 120)]
    [InlineData(GeneralMidiProgram.BreathNoise, 121)]
    [InlineData(GeneralMidiProgram.Seashore, 122)]
    [InlineData(GeneralMidiProgram.BirdTweet, 123)]
    [InlineData(GeneralMidiProgram.TelephoneRing, 124)]
    [InlineData(GeneralMidiProgram.Helicopter, 125)]
    [InlineData(GeneralMidiProgram.Applause, 126)]
    [InlineData(GeneralMidiProgram.Gunshot, 127)]
    public void Program_IsTheProgramChangeValue(GeneralMidiProgram program, byte value)
    {
        Assert.Equal(value, (byte)program);
    }

    [Theory]
    [InlineData(GeneralMidiPercussion.AcousticBassDrum, 35)]
    [InlineData(GeneralMidiPercussion.BassDrum1, 36)]
    [InlineData(GeneralMidiPercussion.SideStick, 37)]
    [InlineData(GeneralMidiPercussion.AcousticSnare, 38)]
    [InlineData(GeneralMidiPercussion.HandClap, 39)]
    [InlineData(GeneralMidiPercussion.ElectricSnare, 40)]
    [InlineData(GeneralMidiPercussion.LowFloorTom, 41)]
    [InlineData(GeneralMidiPercussion.ClosedHiHat, 42)]
    [InlineData(GeneralMidiPercussion.HighFloorTom, 43)]
    [InlineData(GeneralMidiPercussion.PedalHiHat, 44)]
    [InlineData(GeneralMidiPercussion.LowTom, 45)]
    [InlineData(GeneralMidiPercussion.OpenHiHat, 46)]
    [InlineData(GeneralMidiPercussion.LowMidTom, 47)]
    [InlineData(GeneralMidiPercussion.HiMidTom, 48)]
    [InlineData(GeneralMidiPercussion.CrashCymbal1, 49)]
    [InlineData(GeneralMidiPercussion.HighTom, 50)]
    [InlineData(GeneralMidiPercussion.RideCymbal1, 51)]
    [InlineData(GeneralMidiPercussion.ChineseCymbal, 52)]
    [InlineData(GeneralMidiPercussion.RideBell, 53)]
    [InlineData(GeneralMidiPercussion.Tambourine, 54)]
    [InlineData(GeneralMidiPercussion.SplashCymbal, 55)]
    [InlineData(GeneralMidiPercussion.Cowbell, 56)]
    [InlineData(GeneralMidiPercussion.CrashCymbal2, 57)]
    [InlineData(GeneralMidiPercussion.Vibraslap, 58)]
    [InlineData(GeneralMidiPercussion.RideCymbal2, 59)]
    [InlineData(GeneralMidiPercussion.HiBongo, 60)]
    [InlineData(GeneralMidiPercussion.LowBongo, 61)]
    [InlineData(GeneralMidiPercussion.MuteHiConga, 62)]
    [InlineData(GeneralMidiPercussion.OpenHiConga, 63)]
    [InlineData(GeneralMidiPercussion.LowConga, 64)]
    [InlineData(GeneralMidiPercussion.HighTimbale, 65)]
    [InlineData(GeneralMidiPercussion.LowTimbale, 66)]
    [InlineData(GeneralMidiPercussion.HighAgogo, 67)]
    [InlineData(GeneralMidiPercussion.LowAgogo, 68)]
    [InlineData(GeneralMidiPercussion.Cabasa, 69)]
    [InlineData(GeneralMidiPercussion.Maracas, 70)]
    [InlineData(GeneralMidiPercussion.ShortWhistle, 71)]
    [InlineData(GeneralMidiPercussion.LongWhistle, 72)]
    [InlineData(GeneralMidiPercussion.ShortGuiro, 73)]
    [InlineData(GeneralMidiPercussion.LongGuiro, 74)]
    [InlineData(GeneralMidiPercussion.Claves, 75)]
    [InlineData(GeneralMidiPercussion.HiWoodBlock, 76)]
    [InlineData(GeneralMidiPercussion.LowWoodBlock, 77)]
    [InlineData(GeneralMidiPercussion.MuteCuica, 78)]
    [InlineData(GeneralMidiPercussion.OpenCuica, 79)]
    [InlineData(GeneralMidiPercussion.MuteTriangle, 80)]
    [InlineData(GeneralMidiPercussion.OpenTriangle, 81)]
    public void Percussion_IsTheNoteNumber(GeneralMidiPercussion sound, byte note)
    {
        Assert.Equal(note, (byte)sound);
    }

    [Fact]
    public void Enums_HaveEveryValueOnce()
    {
        Assert.Equal(Enumerable.Range(0, 128).Select(i => (byte)i), Enum.GetValues<GeneralMidiProgram>().Select(p => (byte)p));
        Assert.Equal(Enumerable.Range(35, 47).Select(i => (byte)i), Enum.GetValues<GeneralMidiPercussion>().Select(p => (byte)p));
        Assert.Equal(16, Enum.GetValues<GeneralMidiFamily>().Length);
    }

    [Theory]
    [InlineData(GeneralMidiProgram.AcousticGrandPiano, GeneralMidiFamily.Piano)]
    [InlineData(GeneralMidiProgram.Clavi, GeneralMidiFamily.Piano)]
    [InlineData(GeneralMidiProgram.Celesta, GeneralMidiFamily.ChromaticPercussion)]
    [InlineData(GeneralMidiProgram.Violin, GeneralMidiFamily.Strings)]
    [InlineData(GeneralMidiProgram.Timpani, GeneralMidiFamily.Strings)]
    [InlineData(GeneralMidiProgram.Lead8BassAndLead, GeneralMidiFamily.SynthLead)]
    [InlineData(GeneralMidiProgram.Fx1Rain, GeneralMidiFamily.SynthEffects)]
    [InlineData(GeneralMidiProgram.Gunshot, GeneralMidiFamily.SoundEffects)]
    public void GetFamily_GroupsProgramsByEight(GeneralMidiProgram program, GeneralMidiFamily family)
    {
        Assert.Equal(family, GeneralMidi.GetFamily(program));
    }

    [Fact]
    public void GetFamily_EveryProgram_IsItsBlockOfEight()
    {
        foreach (var program in Enum.GetValues<GeneralMidiProgram>())
        {
            Assert.Equal((GeneralMidiFamily)((byte)program / 8), GeneralMidi.GetFamily(program));
        }
    }

    [Fact]
    public void GetFamily_ValueAbove127_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => GeneralMidi.GetFamily((GeneralMidiProgram)128));
    }

    [Fact]
    public void PercussionChannel_IsChannel10()
    {
        Assert.Equal(MidiChannel.Channel10, GeneralMidi.PercussionChannel);
        Assert.Equal(9, (byte)GeneralMidi.PercussionChannel); // status byte nibble
    }

    [Fact]
    public void Casts_BuildMessages()
    {
        var program = new MidiMessageProgramChange(MidiChannel.Channel01, (byte)GeneralMidiProgram.Violin);
        var snare = new MidiMessageNote(GeneralMidi.PercussionChannel, new MidiNote((byte)GeneralMidiPercussion.AcousticSnare), 100, MidiMessageType.NoteOn);

        Assert.Equal([0xC0, 40], MidiMessageConverter.ToBytes(program));
        Assert.Equal([0x99, 38, 100], MidiMessageConverter.ToBytes(snare));
        Assert.Equal(GeneralMidiProgram.Violin, (GeneralMidiProgram)program.Program);
    }
}
