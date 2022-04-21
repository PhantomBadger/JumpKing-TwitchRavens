using Microsoft.Xna.Framework.Input;
using Settings;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JumpKingMod.Install.UI
{
    public class RavensSettings : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Whether the Raven system is enabled or not
        /// </summary>
        public bool RavenEnabled
        {
            get
            {
                return ravenEnabled;
            }
            set
            {
                if (ravenEnabled != value)
                {
                    ravenEnabled = value;
                    RaisePropertyChanged(nameof(RavenEnabled));
                    RaisePropertyChanged(nameof(GunSettingsVisible));
                }
            }
        }
        private bool ravenEnabled;

        /// <summary>
        /// The key to use to toggle the raven spawning
        /// </summary>
        public Keys RavenToggleDebugKey
        {
            get
            {
                return ravenToggleDebugKey;
            }
            set
            {
                if (ravenToggleDebugKey != value)
                {
                    ravenToggleDebugKey = value;
                    RaisePropertyChanged(nameof(RavenToggleDebugKey));
                }
            }
        }
        private Keys ravenToggleDebugKey;

        /// <summary>
        /// The key to use to clear the ravens
        /// </summary>
        public Keys RavenClearDebugKey
        {
            get
            {
                return ravenClearDebugKey;
            }
            set
            {
                if (ravenClearDebugKey != value)
                {
                    ravenClearDebugKey = value;
                    RaisePropertyChanged(nameof(RavenClearDebugKey));
                }
            }
        }
        private Keys ravenClearDebugKey;

        /// <summary>
        /// The key to use to toggle sub mode
        /// </summary>
        public Keys RavenSubModeToggleKey
        {
            get
            {
                return ravenSubModeToggleKey;
            }
            set
            {
                if (ravenSubModeToggleKey != value)
                {
                    ravenSubModeToggleKey = value;
                    RaisePropertyChanged(nameof(RavenSubModeToggleKey));
                }
            }
        }
        private Keys ravenSubModeToggleKey;

        /// <summary>
        /// The trigger type we want to use for the ravens on Twitch
        /// </summary>
        public TwitchRavenTriggerTypes RavenTriggerType
        {
            get
            {
                return ravenTriggerType;
            }
            set
            {
                if (ravenTriggerType != value)
                {
                    ravenTriggerType = value;
                    RaisePropertyChanged(nameof(RavenTriggerType));
                }
            }
        }
        private TwitchRavenTriggerTypes ravenTriggerType;

        /// <summary>
        /// The trigger type we want to use for the ravens on YouTube
        /// </summary>
        public YouTubeRavenTriggerTypes YouTubeRavenTriggerType
        {
            get
            {
                return youTubeRavenTriggerType;
            }
            set
            {
                if (youTubeRavenTriggerType != value)
                {
                    youTubeRavenTriggerType = value;
                    RaisePropertyChanged(nameof(YouTubeRavenTriggerType));
                }
            }
        }
        private YouTubeRavenTriggerTypes youTubeRavenTriggerType;

        /// <summary>
        /// The maximum number of ravens visible on the screen at once
        /// </summary>
        public string MaxRavensCount
        {
            get
            {
                return maxRavensCount.ToString();
            }
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    maxRavensCount = 0;
                }
                else
                {
                    if (int.TryParse(value, out int newVal))
                    {
                        if (newVal < 0)
                        {
                            newVal = Math.Abs(newVal);
                        }
                        if (newVal > 100)
                        {
                            newVal = 100;
                        }
                        maxRavensCount = newVal;
                    }
                }
                RaisePropertyChanged(nameof(MaxRavensCount));
            }
        }
        private int maxRavensCount;

        /// <summary>
        /// The ID of the Channel Point Reward to use for the Raven Trigger
        /// </summary>
        public string RavensChannelPointID
        {
            get
            {
                return ravensChannelPointID;
            }
            set
            {
                if (ravensChannelPointID != value)
                {
                    ravensChannelPointID = value;
                    RaisePropertyChanged(nameof(RavensChannelPointID));
                }
            }
        }
        private string ravensChannelPointID;

        /// <summary>
        /// A collection of excluded terms
        /// </summary>
        public ObservableCollection<string> ExcludedTerms
        {
            get
            {
                return excludedTerms;
            }
            set
            {
                if (excludedTerms != value)
                {
                    excludedTerms = value;
                    RaisePropertyChanged(nameof(ExcludedTerms));
                }
            }
        }
        private ObservableCollection<string> excludedTerms;

        /// <summary>
        /// The index of the selected item in the excluded items list
        /// </summary>
        public int SelectedExcludedItemIndex
        {
            get
            {
                return selectedExcludedItemIndex;
            }
            set
            {
                if (selectedExcludedItemIndex != value)
                {
                    selectedExcludedItemIndex = value;
                    RaisePropertyChanged(nameof(SelectedExcludedItemIndex));
                }
            }
        }
        private int selectedExcludedItemIndex;

        /// <summary>
        /// The candidate item to add to the excluded items list
        /// </summary>
        public string CandidateExcludedItem
        {
            get
            {
                return candidateExcludedItem;
            }
            set
            {
                if (candidateExcludedItem != value)
                {
                    candidateExcludedItem = value;
                    RaisePropertyChanged(nameof(CandidateExcludedItem));
                }
            }
        }
        private string candidateExcludedItem;

        /// <summary>
        /// A collection of raven insults
        /// </summary>
        public ObservableCollection<string> RavenInsults
        {
            get
            {
                return ravenInsults;
            }
            set
            {
                if (ravenInsults != value)
                {
                    ravenInsults = value;
                    RaisePropertyChanged(nameof(RavenInsults));
                }
            }
        }
        private ObservableCollection<string> ravenInsults;

        /// <summary>
        /// The index of the selected item in the Raven Insults list
        /// </summary>
        public int SelectedRavenInsultIndex
        {
            get
            {
                return selectedRavenInsultIndex;
            }
            set
            {
                if (selectedRavenInsultIndex != value)
                {
                    selectedRavenInsultIndex = value;
                    RaisePropertyChanged(nameof(SelectedRavenInsultIndex));
                }
            }
        }
        private int selectedRavenInsultIndex;

        /// <summary>
        /// The candidate item to add to the Raven Insults list
        /// </summary>
        public string CandidateRavenInsult
        {
            get
            {
                return candidateRavenInsult;
            }
            set
            {
                if (candidateRavenInsult != value)
                {
                    candidateRavenInsult = value;
                    RaisePropertyChanged(nameof(CandidateRavenInsult));
                }
            }
        }
        private string candidateRavenInsult;

        /// <summary>
        /// The number of ravens to spawn when the insult trigger is hit
        /// </summary>
        public string InsultRavenSpawnCount
        {
            get
            {
                return insultRavenSpawnCount.ToString();
            }
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    insultRavenSpawnCount = 0;
                }
                else
                {
                    if (int.TryParse(value, out int newVal))
                    {
                        if (newVal < 0)
                        {
                            newVal = Math.Abs(newVal);
                        }
                        if (newVal > 100)
                        {
                            newVal = 100;
                        }
                        insultRavenSpawnCount = newVal;
                    }
                }
                RaisePropertyChanged(nameof(InsultRavenSpawnCount));
            }
        }
        private int insultRavenSpawnCount;

        /// <summary>
        /// Whether the Gun mode can be enabled or not
        /// </summary>
        public bool GunEnabled
        {
            get
            {
                return gunEnabled;
            }
            set
            {
                if (gunEnabled != value)
                {
                    gunEnabled = value;
                    RaisePropertyChanged(nameof(GunEnabled));
                    RaisePropertyChanged(nameof(GunSettingsVisible));
                }
            }
        }
        private bool gunEnabled;

        public bool GunSettingsVisible
        {
            get
            {
                return gunEnabled && ravenEnabled;
            }
        }

        /// <summary>
        /// The key to press to toggle the gun mode
        /// </summary>
        public Keys GunToggleKey
        {
            get
            {
                return gunToggleKey;
            }
            set
            {
                if (gunToggleKey != value)
                {
                    gunToggleKey = value;
                    RaisePropertyChanged(nameof(GunToggleKey));
                }
            }
        }
        private Keys gunToggleKey;

        /// <summary>
        /// Constructor for creating a <see cref="RavensSettings"/>
        /// </summary>
        public RavensSettings()
        {
            ExcludedTerms = new ObservableCollection<string>();
            RavenInsults = new ObservableCollection<string>();
        }

        /// <summary>
        /// Invokes the <see cref="PropertyChanged"/> event
        /// </summary>
        public void RaisePropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
