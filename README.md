# Status

[SonarDashboardLink]: https://sonarcloud.io/dashboard?id=Daniel127_RtMidi.Net
[![Quality Gate](https://sonarcloud.io/api/project_badges/measure?project=Daniel127_RtMidi.Net&metric=alert_status)][SonarDashboardLink] [![Lines of Code](https://sonarcloud.io/api/project_badges/measure?project=Daniel127_RtMidi.Net&metric=ncloc)][SonarDashboardLink] [![Bugs](https://sonarcloud.io/api/project_badges/measure?project=Daniel127_RtMidi.Net&metric=bugs)][SonarDashboardLink] [![Coverage](https://sonarcloud.io/api/project_badges/measure?project=Daniel127_RtMidi.Net&metric=coverage)][SonarDashboardLink] [![Maintainability Rating](https://sonarcloud.io/api/project_badges/measure?project=Daniel127_RtMidi.Net&metric=sqale_rating)][SonarDashboardLink]

[GitHubBadgeMaster]: https://github.com/Daniel127/RtMidi.Net/workflows/Build/badge.svg?branch=main
[GitHubBadgeDevelop]: https://github.com/Daniel127/RtMidi.Net/workflows/Build/badge.svg?branch=develop
[GitHubActionsLink]: https://github.com/Daniel127/RtMidi.Net/actions?query=workflow%3ABuild

[AzureBadgeMaster]: https://dev.azure.com/Daniel127/RtMidi.Net/_apis/build/status/CI-Release?branchName=main
[AzurePipelineMaster]: https://dev.azure.com/Daniel127/RtMidi.Net/_build/latest?definitionId=12&branchName=main
[AzureBadgeDevelop]: https://dev.azure.com/Daniel127/RtMidi.Net/_apis/build/status/CI-Development?branchName=develop
[AzurePipelineDevelop]: https://dev.azure.com/Daniel127/RtMidi.Net/_build/latest?definitionId=11&branchName=develop

[NugetUrl]: https://www.nuget.org/packages/RtMidi.Net
[NugetBadge]: https://feeds.dev.azure.com/Daniel127/8422bbcb-06b4-4da0-bb49-3b51aab2d4bc/_apis/public/Packaging/Feeds/RtMidi.Net/Packages/5bb5e570-4239-4a73-b928-d291ec05a9e3/Badge

[RtMidiUrl]: https://github.com/thestk/rtmidi

| Branch | Build | Deployment |
|:----:|:-------------:|:----:|
| main | [![Build][GitHubBadgeMaster]][GitHubActionsLink]  [![Build Status][AzureBadgeMaster]][AzurePipelineMaster] | [![Nuget package][NugetBadge]][NugetUrl] |
| develop | [![Build][GitHubBadgeDevelop]][GitHubActionsLink]  [![Build Status][AzureBadgeDevelop]][AzurePipelineDevelop] | N/A |


# What is it?

This project is a .NET wrapper for the [RtMidi][RtMidiUrl] project.

I have created it to use it in an own project in RaspberryPi with a very basic utility as it is the reading of the notes, nevertheless I have done it thinking of being able to use all the options of the MIDI protocol, although I have not come to test them all.

If you find any error or have any improvement do not hesitate to make a PR.

# How to use?

To connect to a device you can use the ``MidiInputClient`` and ``MidiOutputClient`` classes, ``MidiManager`` has useful methods to know the environment.

There is a project (WorkerTest) to test the key reading but it really receives any kind of MIDI message so don't hesitate to extend it if you need it.

If you subscribe to the ``OnMessageReceived`` event of the ``MidiInputClient`` don't forget to use ``ActivateMessageReceivedEvent`` for this event to work, this is because RtMidi includes a queue to store the events and later you can read the events with ``GetMessage``, if you need to stop reading the events in real time you can use ``DeactivateMessageReceivedEvent`` to deactivate the event.

```csharp
uint devicePort = 0;
var device = MidiManager.GetDeviceInfo(devicePort, RtMidi.Net.Enums.MidiDeviceType.Input);
MidiInputClient midiInputClient = new MidiInputClient(device);
midiInputClient.OnMessageReceived += MidiClient_OnMessageReceived;
midiInputClient.ActivateMessageReceivedEvent();
midiInputClient.Open();

//...wait or do something...

midiInputClient.Close();
midiInputClient.Dispose();
```