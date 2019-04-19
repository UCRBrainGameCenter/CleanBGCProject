using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using BGC.DataStructures.Generic;
using BGC.Mathematics;
using BGC.Audio;
using BGC.Audio.Envelopes;
using BGC.Audio.Synthesis;
using BGC.Users;
using BGC.UI.Panels;
using BGC.UI.Dialogs;

namespace TestA
{
    public class SettingsDemo : ModePanel
    {
        [Header("Engine References")]
        [SerializeField]
        private MenuManager menuManager = null;

        [Header("Interface References")]
        [SerializeField]
        private Button quitButton = null;
        [SerializeField]
        private Button doneButton = null;
        [SerializeField]
        private Image[] globalColorImages = null;
        [SerializeField]
        private Image[] userColorImages = null;
        [SerializeField]
        private Text[] boolDisplayTexts = null;
        [SerializeField]
        private Text stringDisplayText = null;
        [SerializeField]
        private Text statsDisplayText = null;
        [SerializeField]
        private Toggle trackingClicksToggle = null;

        [Header("Dynamic Menu Targets")]
        [SerializeField]
        private Text[] textElements = null;
        [SerializeField]
        private Image[] imageElements = null;
        [SerializeField]
        private RectTransform[] frameElements = null;

        [Header("Audio References")]
        [SerializeField]
        private BGCClipPlayer streamPlayer = null;

        private const string EntryKey = "TestA.Entries";
        private const string ExitKey = "TestA.Exits";
        private const string ClicksKey = "TestA.Clicks";

        private TestAClickChannel clickChannel = null;
        private TestALerpChannel saveDataChannel = null;
        private readonly List<TestALerpChannel> messAroundLerpChannels = new List<TestALerpChannel>(5);
        private DepletableBag<RectTransform> driftableFrames = null;

        public int EntryCount
        {
            get => PlayerData.GetInt(EntryKey, 0);
            private set
            {
                PlayerData.SetInt(EntryKey, value);
                UpdateStatsText();
            }
        }

        public int ExitCount
        {
            get => PlayerData.GetInt(ExitKey, 0);
            private set
            {
                PlayerData.SetInt(ExitKey, value);
                UpdateStatsText();
            }
        }

        public int ClicksCount
        {
            get => PlayerData.GetInt(ClicksKey, 0);
            private set
            {
                PlayerData.SetInt(ClicksKey, value);
                UpdateStatsText();
            }
        }

        private static readonly string[] globalColorKeys = new string[]
        {
            SettingsMenu.Keys.ExampleGlobalColor1,
            SettingsMenu.Keys.ExampleGlobalColor2,
            SettingsMenu.Keys.ExampleGlobalColor3
        };

        private static readonly string[] userColorKeys = new string[]
        {
            SettingsMenu.Keys.ExampleUserColor1,
            SettingsMenu.Keys.ExampleUserColor2,
            SettingsMenu.Keys.ExampleUserColor3
        };

        private static readonly string[] boolKeys = new string[]
        {
            SettingsMenu.Keys.ExampleOpenBoolean,
            SettingsMenu.Keys.ExampleAdminBoolean,
            SettingsMenu.Keys.ExampleLockedBoolean
        };

        private void Awake()
        {
            quitButton.onClick.AddListener(QuitClicked);
            doneButton.onClick.AddListener(DoneClicked);

            trackingClicksToggle.onValueChanged.AddListener(TrackingClicksToggled);

            Debug.Assert(globalColorImages != null &&
                globalColorImages.Length == globalColorKeys.Length);

            Debug.Assert(userColorImages != null &&
                userColorImages.Length == userColorKeys.Length);

            Debug.Assert(boolDisplayTexts != null &&
                boolDisplayTexts.Length == boolKeys.Length);

            Debug.Assert(stringDisplayText != null);

            Debug.Assert(statsDisplayText != null);

            clickChannel = gameObject.AddComponent<TestAClickChannel>();

            saveDataChannel = gameObject.AddComponent<TestALerpChannel>();

            for (int i = 0; i < 5; i++)
            {
                messAroundLerpChannels.Add(gameObject.AddComponent<TestALerpChannel>());
            }

            driftableFrames = new DepletableBag<RectTransform>(frameElements);
        }

        public override void FocusAcquired()
        {
            //
            // Load in the settings values
            //

            //Classic
            for (int i = 0; i < globalColorKeys.Length; i++)
            {
                globalColorImages[i].color = SettingsMenu.GetSettingColor(globalColorKeys[i]);
            }

            //With Linq
            foreach ((string key, Image image) in userColorKeys.Zip(userColorImages, (x, y) => (x, y)))
            {
                image.color = SettingsMenu.GetSettingColor(key);
            }

            //Slightly Different Linq
            foreach (var pair in boolKeys.Zip(boolDisplayTexts, (key, textElem) => (key, textElem)))
            {
                pair.textElem.text = SettingsMenu.GetSettingBool(pair.key).ToString();
            }

            stringDisplayText.text = SettingsMenu.GetSettingString(SettingsMenu.Keys.ExampleString);

            //
            // Handle special settings
            //

            if (SettingsMenu.GetSettingBool(SettingsMenu.Keys.PlayAnnoyingSound))
            {
                // Kick off "Music"
                PlayAnnoyingSound();
            }

            if (SettingsMenu.GetSettingBool(SettingsMenu.Keys.MessWithSettingsDemo))
            {
                driftableFrames.Reset();
                // Kick off "Dynamic Menu" in 0.5 seconds
                KickOffMessAround();
            }

            //
            // Update some UserData
            //

            EntryCount++;

            //Enable click tracking
            trackingClicksToggle.isOn = true;
            clickChannel.Activate(x => x.ClicksCount++);

            //Enable periodic automatic saving
            SaveAndRelaunch(this);
        }

        private void SaveAndRelaunch(SettingsDemo _settingsDemo)
        {
            PlayerData.Save();

            saveDataChannel.Activate(
                duration: 2f,
                finishedCallback: SaveAndRelaunch,
                interruptedCallback: x => PlayerData.Save());
        }

        private void PlayAnnoyingSound()
        {
            //Indefinite Spectrotemporal modulation with 60Hz amplitude modulation
            streamPlayer.PlayStream(
                new STMAudioClip(
                    duration: 2.0,
                    freqLB: 20.0,
                    freqUB: 4000.0,
                    frequencyCount: 10000,
                    modulationDepth: 20.0,
                    spectralModulationRate: 2.0,
                    temporalModulationRate: 2.0,
                    rippleDirection: STMAudioClip.RippleDirection.Up,
                    distribution: STMAudioClip.AmplitudeDistribution.Pink)
                .Loop()
                .ApplyEnvelope(
                    new SineWave(
                        amplitude: 1.0,
                        frequency: 60.0))
                .ApplyEnvelope(
                    new EnvelopeConcatenator(
                        CosineEnvelope.HammingWindow(0.03, true),
                        new ConstantEnvelope(1.0)))
                .Normalize(80.0));
        }

        private void KickOffMessAround()
        {
            //Start the Text manipulation after 1 second
            messAroundLerpChannels[0].Activate(
                duration: 1f,
                finishedCallback: _ => MessUpText());

            //Start the Color manipulation after 2 seconds
            messAroundLerpChannels[1].Activate(
                duration: 2f,
                finishedCallback: _ => DriftingColor());

            //Start the slow drift after 3 seconds
            messAroundLerpChannels[2].Activate(
                duration: 3f,
                finishedCallback: _ => DriftingFrame(2));

            //Start the medium drift after 4 seconds
            messAroundLerpChannels[3].Activate(
                duration: 4f,
                finishedCallback: _ => DriftingFrame(3));

            //Start the large drift after 5 seconds
            messAroundLerpChannels[4].Activate(
                duration: 5f,
                finishedCallback: _ => DriftingFrame(4));
        }

        private void MessUpText()
        {
            float randomValue = CustomRandom.NextFloat();

            if (randomValue < 0.25f)
            {
                //25% Random fast text breaking

                //Select target
                Text target = textElements[CustomRandom.Next(0, textElements.Length)];

                string correctText = target.text;
                string newText = new string(correctText.Select(Shuffle).ToArray());

                target.text = newText;

                messAroundLerpChannels[0].Activate(
                    duration: 0.05f,
                    finishedCallback: _ =>
                    {
                        target.text = correctText;
                        MessUpText();
                    },
                    interruptedCallback: x => target.text = correctText);
            }
            else
            {
                //75% constant text shuffle

                //Select target
                Text target = textElements[CustomRandom.Next(0, textElements.Length)];

                string correctText = target.text;

                if (target.text.Length < 2)
                {
                    target.text = "aa";
                }

                messAroundLerpChannels[0].Activate(
                    duration: 0.25f,
                    continuousAction: new TextShuffler(target),
                    finishedCallback: _ =>
                    {
                        target.text = correctText;
                        MessUpText();
                    },
                    interruptedCallback: x => target.text = correctText);
            }
        }

        private void DriftingColor()
        {
            //Select target
            Image target = imageElements[CustomRandom.Next(0, imageElements.Length)];

            Color originalColor = target.color;
            Color targetColor = CustomRandom.NextDouble() < 0.5 ? originalColor * 1.25f : originalColor / 1.25f;

            messAroundLerpChannels[1].Activate(
                duration: 0.125f,
                lerpAction: new ImageColorDrifter(target, targetColor),
                finishedCallback: _ =>
                {
                    target.color = originalColor;
                    DriftingColor();
                },
                interruptedCallback: x => target.color = originalColor);
        }

        private void DriftingFrame(int index)
        {
            float duration;
            float distance;

            switch (index)
            {
                case 2:
                    //Long and Continuous
                    duration = 1f;
                    distance = 1f;
                    break;

                case 3:
                    //Medium
                    duration = 0.5f;
                    distance = 2f;
                    break;

                case 4:
                    //Medium
                    duration = 0.25f;
                    distance = 4f;
                    break;

                default:
                    Debug.LogError($"Unexpected Index for DriftingFrame: {index}");
                    return;
            }

            //Select target
            RectTransform target = driftableFrames.PopNext();

            Vector2 originalPosition = target.localPosition;

            messAroundLerpChannels[index].Activate(
                duration: duration,
                continuousAction: new GameObjectDrifter(target, distance),
                finishedCallback: _ =>
                {
                    driftableFrames.ReplenishValue(target);
                    target.localPosition = originalPosition;
                    DriftingFrame(index);
                },
                interruptedCallback: x => target.localPosition = originalPosition);
        }


        private static char Shuffle(char c)
        {
            if (c >= 'A' && c <= 'Z')
            {
                //Random Capital Letter
                return (char)('A' + CustomRandom.Next(0, 'Z' - 'A'));
            }
            else if (c >= '0' && c <= '9')
            {
                //Random Number
                return (char)('0' + CustomRandom.Next(0, '9' - '0'));
            }

            //Random Lowercase Letter
            return (char)('a' + CustomRandom.Next(0, 'z' - 'a'));
        }

        public override void FocusLost()
        {
            streamPlayer.Stop();

            ExitCount++;

            saveDataChannel.Kill();
            messAroundLerpChannels.ForEach(x => x.Kill());
        }

        private void UpdateStatsText()
        {
            statsDisplayText.text = $"TestA - Entries: {EntryCount}, Exits: {ExitCount}, Clicks: {ClicksCount}";
        }

        #region Click Callbacks

        private void QuitTask()
        {
            menuManager.PopWindowState();
        }

        private void TrackingClicksToggled(bool value)
        {
            if (value)
            {
                clickChannel.Activate(x => x.ClicksCount++);
            }
            else
            {
                ClicksCount++;
                clickChannel.StripCallback();
            }
        }

        private void DoneClicked()
        {
            ClicksCount++;
            menuManager.PopWindowState();
        }

        private void QuitClicked()
        {
            ClicksCount++;

            ModalDialog.ShowSimpleModal(ModalDialog.Mode.ConfirmCancel,
                headerText: "Quit Task",
                bodyText: "Are you sure you want to quit the task?",
                callback: (response) =>
                {
                    switch (response)
                    {
                        case ModalDialog.Response.Confirm:
                            QuitTask();
                            break;

                        case ModalDialog.Response.Cancel:
                            //Do Nothing
                            break;

                        default:
                            Debug.LogError($"Unexpected ModalDialog.Response: {response}");
                            break;
                    }
                });
        }

        #endregion Click Callbacks
    }
}
