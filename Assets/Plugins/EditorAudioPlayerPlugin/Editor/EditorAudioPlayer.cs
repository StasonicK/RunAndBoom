using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Windows;

namespace MBS.Tools
{
    public class EditorAudioPlayer : EditorWindow
    {
        private AudioClip m_Sound;
        private static float m_SoundVolume = 1;
        private float m_ClipProgress;
        private float m_NewClipProgress;
        private float m_ClipLength;
        private bool m_LoopSelection;
        private bool m_PopulateWithFilesSelected;
        private bool m_PlaySelectedWhenSelected;
        private bool m_IsPlaying;
        private bool m_IsPaused;
        private bool m_tempHoldOnSoftStop;

        private GameObject m_CurrentSoundObj;
        private AudioSource m_CurrentAudioSource;
        private AudioClip m_LastSound;
        private List<AudioClip> m_SelectedSounds;
        private int m_CurrentSoundIndex;
        private bool inPlayModeLastFrame;
        private GameObject m_SoundObjForDestroyWhenExitingPlayMode;

        private static Texture2D m_MediaPlayButtonTexture;
        private static Texture2D m_MediaPauseButtonTexture;
        private static Texture2D m_MediaStopButtonTexture;
        private static Texture2D m_MediaNextButtonTexture;
        private static Texture2D m_MediaPreviousButtonTexture;
        private GUIContent m_MediaPlayButtonContex;
        private GUIContent m_MediaPauseButtonContext;
        private GUIContent m_MediaPauseActiveButtonContext;
        private GUIContent m_MediaStopButtonContex;
        private GUIContent m_MediaNextButtonContex;
        private GUIContent m_MediaPreviousButtonContex;

        private static GUIStyle m_ToggleButtonStyleNormal = null;
        private static GUIStyle m_ToggleButtonStyleToggled = null;

        private string m_PlaybackTextFieldInput;
        private string m_VolumeTextFieldInput;
        private bool m_IsChangingPlaybackText;
        private bool m_IsChangingVolumeText;

        private Color m_DefaultColor;
        private Color m_Red;
        private Color m_Green;
        private Color m_DullGreen;
        private Color m_Yellow;
        private Color m_DullYellow;

        private string m_PlayButtonTooltip;
        private string m_PauseButtonTooltip;
        private string m_UnpauseButtonTooltip;
        private string m_StopButtonTooltip;
        private string m_PlayPreviousButtonTooltip;
        private string m_PlayNextButtonTooltip;
        private string m_CurrentPlaybackProgressTooltip;
        private string m_PlaybackProgressTotalLengthTooltip;
        private string m_VolumeTooltip;
        private string m_VolumePercentTooltip;
        private string m_LoopSelectionTooltip;
        private string m_PlaySelectedFileTooltip;
        private string m_PlayFileWhenSelectedTooltip;
        private string m_ClipFileTooltip;
        private string m_ClipPlayingTitleTootip;
        private string m_PlaylistTitleTooltip;
        private string m_PlaylistItemTooltip;

        [MenuItem("Tools/Madboy Studios/Editor Audio Player")]
        private static void Init()
        {
            EditorAudioPlayer window = GetWindow<EditorAudioPlayer>();
            window.titleContent = new GUIContent("Editor Audio Player");
            window.minSize = new Vector2(300f, 120f);
            window.Show();
        }

        private void OnGUI()
        {
            GUIInit();

            DrawTitle();

            DrawControlRow();

            GUILayout.Label("");

            DrawPlaybackProgressSlider();

            GUILayout.Label("");

            DrawVolumeSlider();

            GUILayout.Label("");

            //Looping Option
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            DrawLoopOption();

            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            //PopulateWithFileSelect Option
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            DrawPopulateWithFileSelectionOption();

            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            //PlayFileOnSelect Option
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            DrawPlayOnFileSelectionOption();

            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();


            GUILayout.Label("");

            DrawClip();

            DrawPlaylist();
        }

        private void GUIInit()
        {
            m_PlayButtonTooltip = "Play";
            m_PauseButtonTooltip = "Pause";
            m_UnpauseButtonTooltip = "Unpause";
            m_StopButtonTooltip = "Stop";
            m_PlayPreviousButtonTooltip = "Previous";
            m_PlayNextButtonTooltip = "Next";
            m_CurrentPlaybackProgressTooltip = "Current Time";
            m_PlaybackProgressTotalLengthTooltip = "Total Time";
            m_VolumeTooltip = "Volume";
            m_VolumePercentTooltip = "Volume Percent";
            m_LoopSelectionTooltip = "Loop Clip/ Playlist Selection";
            m_PlaySelectedFileTooltip = "Queue Files that are selected in the Project Tab";
            m_PlayFileWhenSelectedTooltip = "Auto Play files that are selected in the project tab as they are selected";
            m_ClipFileTooltip = "Current audio file";
            m_ClipPlayingTitleTootip = "Title of current audio file";
            m_PlaylistTitleTooltip = "Playlist of selected audio files";
            m_PlaylistItemTooltip = "Playlisted audio file";

            m_DefaultColor = GUI.color;
            m_Red = new Color(255, .5f, .5f, 1);
            m_Green = new Color(.5f, 2, .5f, 1);
            m_DullGreen = new Color(.5f, 1.25f, .5f, 1);
            m_Yellow = new Color(2, 2, 0, 1);
            m_DullYellow = new Color(1.5f, 1.5f, 1.1f, 1);

            if (m_ToggleButtonStyleNormal == null)
            {
                //setup button toggle styles
                m_ToggleButtonStyleNormal = "Button";
                m_ToggleButtonStyleToggled = new GUIStyle(m_ToggleButtonStyleNormal);
                m_ToggleButtonStyleToggled.normal.background = m_ToggleButtonStyleToggled.active.background;
            }

            if (m_MediaPlayButtonTexture == null)
            {

                string[] folderAsset = AssetDatabase.FindAssets("t:Folder MadboyStudios");
                string path = AssetDatabase.GUIDToAssetPath(folderAsset[0]);

                var assets = AssetDatabase.FindAssets("t:texture2D", new[] { path }).
                    Select(AssetDatabase.GUIDToAssetPath).
                    Select(AssetDatabase.LoadAssetAtPath<Texture2D>).
                    Where(x => (x.name.Contains("Media")));

                foreach (var asset in assets)
                {
                    switch (asset.name)
                    {
                        case "MediaPlay":
                            m_MediaPlayButtonTexture = asset;
                            break;

                        case "MediaPause":
                            m_MediaPauseButtonTexture = asset;
                            break;

                        case "MediaStop":
                            m_MediaStopButtonTexture = asset;
                            break;

                        case "MediaNext":
                            m_MediaNextButtonTexture = asset;
                            break;

                        case "MediaPrevious":
                            m_MediaPreviousButtonTexture = asset;
                            break;
                    }
                }
            }

            if (m_MediaPlayButtonContex == null)
            {
                m_MediaPlayButtonContex = new GUIContent(m_MediaPlayButtonTexture, m_PlayButtonTooltip);
                m_MediaPauseButtonContext = new GUIContent(m_MediaPauseButtonTexture, m_PauseButtonTooltip);
                m_MediaPauseActiveButtonContext = new GUIContent(m_MediaPlayButtonTexture, m_UnpauseButtonTooltip);
                m_MediaStopButtonContex = new GUIContent(m_MediaStopButtonTexture, m_StopButtonTooltip);
                m_MediaNextButtonContex = new GUIContent(m_MediaNextButtonTexture, m_PlayNextButtonTooltip);
                m_MediaPreviousButtonContex = new GUIContent(m_MediaPreviousButtonTexture, m_PlayPreviousButtonTooltip);
            }

        }

        private void DrawTitle()
        {
            //Title
            GUILayout.Label("");
            GUILayout.BeginHorizontal();
            int previousFontsize = GUI.skin.label.fontSize;
            GUILayout.FlexibleSpace();
            GUI.skin.label.fontSize = 20;
            GUILayout.Label("Editor Audio Player");
            GUI.skin.label.fontSize = previousFontsize;
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            GUILayout.Label("");
        }

        private void DrawControlRow()
        {

            //Play/Stop button
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            //Play Button
            if (!m_IsPlaying)
            {
                GUI.color = m_Green;
                if (GUILayout.Button(m_MediaPlayButtonContex, GUILayout.MaxHeight(40)))
                {
                    Play();
                }
                GUI.color = m_DefaultColor;
            }
            else
            {
                GUIContent pauseContext = m_IsPaused ? m_MediaPauseActiveButtonContext : m_MediaPauseButtonContext;


                GUI.color = m_IsPaused ? m_Yellow : m_DullYellow;

                if (GUILayout.Button(pauseContext, GUILayout.MaxHeight(40)))
                {
                    Pause();
                }
                if (m_IsPaused)
                    GUI.color = m_DefaultColor;

            }
            //Stop button
            GUI.enabled = m_IsPlaying;
            GUI.color = m_Red;
            if (GUILayout.Button(m_MediaStopButtonContex, GUILayout.MaxHeight(40)))
            {
                Stop(true);
            }
            GUI.color = m_DefaultColor;

            //Previous button
            GUI.enabled = !(m_SelectedSounds == null || m_SelectedSounds.Count <= 1);

            if (GUILayout.Button(m_MediaPreviousButtonContex, GUILayout.MaxHeight(40)))
            {
                PlayPrevious();
            }

            //Next button
            if (GUILayout.Button(m_MediaNextButtonContex, GUILayout.MaxHeight(40)))
            {
                PlayNext();
            }
            GUI.enabled = true;

            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }

        private void DrawPlaybackProgressSlider()
        {
            Event ev = Event.current;

            //playback slider
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            GUILayout.Label(new GUIContent("Playback Progress " + m_ClipProgress + "s", m_CurrentPlaybackProgressTooltip), GUILayout.MaxWidth(150));
            Rect clipProgressRect = GUILayoutUtility.GetLastRect();
            clipProgressRect.size = new Vector2(30, clipProgressRect.size.y);
            clipProgressRect.position = new Vector2(clipProgressRect.position.x + 110, clipProgressRect.position.y);

            m_NewClipProgress = GUILayout.HorizontalSlider(m_NewClipProgress, 0, m_ClipLength, GUILayout.MaxWidth(150));
            GUILayout.Label(new GUIContent(m_ClipLength + " Seconds", m_PlaybackProgressTotalLengthTooltip), GUILayout.MaxWidth(100));

            //Drawing the input field on mouse-over
            if (clipProgressRect.Contains(ev.mousePosition))
            {
                if (!m_IsChangingPlaybackText)
                {
                    m_PlaybackTextFieldInput = m_ClipProgress.ToString();
                    Pause();
                }

                m_IsChangingPlaybackText = true;
                m_PlaybackTextFieldInput = GUI.TextField(clipProgressRect, m_PlaybackTextFieldInput);

                if (ev.keyCode == KeyCode.Return)
                {
                    ValidatePlaybackTextField();
                    m_PlaybackTextFieldInput = m_NewClipProgress.ToString();
                    m_IsChangingPlaybackText = true;
                }


            }
            else if (m_IsChangingPlaybackText && ev.rawType == EventType.Repaint)
            {
                ValidatePlaybackTextField();
            }

            //If no clip is playing, and the playback progress bar was dragged, then start playing the clip
            if (!m_IsPlaying && m_NewClipProgress > 0)
                Play();

            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }

        private void ValidatePlaybackTextField()
        {
            m_IsChangingPlaybackText = false;
            float textInputFloat;
            if (float.TryParse(m_PlaybackTextFieldInput, out textInputFloat))
            {
                float max = m_Sound == null ? 0 : m_Sound.length - .1f;
                m_NewClipProgress = Mathf.Clamp(textInputFloat, 0, max);
            }
            m_PlaybackTextFieldInput = "0";
            Pause();
        }

        private void DrawVolumeSlider()
        {
            Event ev = Event.current;

            //Volume
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            GUILayout.Label(new GUIContent("                           Volume", m_VolumeTooltip), GUILayout.MaxWidth(150));
            m_SoundVolume = GUILayout.HorizontalSlider(m_SoundVolume, 0, 1, GUILayout.MaxWidth(150));
            GUILayout.Label(new GUIContent($"{Mathf.RoundToInt(m_SoundVolume * 100)}%                      ", m_VolumePercentTooltip), GUILayout.MaxWidth(100));

            Rect volumeRect = GUILayoutUtility.GetLastRect();
            volumeRect.size = new Vector2(30, volumeRect.size.y);
            volumeRect.position = new Vector2(volumeRect.position.x, volumeRect.position.y);

            if (volumeRect.Contains(ev.mousePosition))
            {
                if (!m_IsChangingVolumeText)
                    m_VolumeTextFieldInput = Mathf.Round((m_SoundVolume * 100)).ToString();

                m_IsChangingVolumeText = true;
                m_VolumeTextFieldInput = GUI.TextField(volumeRect, m_VolumeTextFieldInput);

                if (ev.keyCode == KeyCode.Return)
                {
                    ValidateVolumeTextField();
                    m_VolumeTextFieldInput = Mathf.Round((m_SoundVolume * 100)).ToString();
                    m_IsChangingVolumeText = true;
                }

            }
            else if (m_IsChangingVolumeText && ev.rawType == EventType.Repaint)
            {
                ValidateVolumeTextField();
            }

            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }

        private void ValidateVolumeTextField()
        {
            m_IsChangingVolumeText = false;
            float textInputFloat;
            if (float.TryParse(m_VolumeTextFieldInput, out textInputFloat))
            {
                m_SoundVolume = Mathf.Clamp01(textInputFloat / 100);

            }
            m_VolumeTextFieldInput = "0";
        }

        private void DrawLoopOption()
        {
            if (m_LoopSelection)
            {
                GUI.color = m_DullGreen;
            }

            if (GUILayout.Button(new GUIContent("Loop Selection: \n" + (m_LoopSelection ? " ON " : " OFF "), m_LoopSelectionTooltip), m_LoopSelection ? m_ToggleButtonStyleToggled : m_ToggleButtonStyleNormal, GUILayout.MaxWidth(150), GUILayout.MinHeight(40)))
            {
                m_LoopSelection = !m_LoopSelection;
            }

            GUI.color = m_DefaultColor;
        }
        private void DrawPopulateWithFileSelectionOption()
        {
            if (m_PopulateWithFilesSelected)
            {
                GUI.color = m_DullGreen;
            }

            if (GUILayout.Button(new GUIContent("Play Selected Files: \n" + (m_PopulateWithFilesSelected ? " ON " : " OFF "), m_PlaySelectedFileTooltip), m_PopulateWithFilesSelected ? m_ToggleButtonStyleToggled : m_ToggleButtonStyleNormal,
                GUILayout.MaxWidth(150), GUILayout.MinHeight(40)))
            {
                m_PopulateWithFilesSelected = !m_PopulateWithFilesSelected;
            }

            GUI.color = m_DefaultColor;
        }
        private void DrawPlayOnFileSelectionOption()
        {
            if (!m_PopulateWithFilesSelected)
                GUI.enabled = false;

            if (m_PlaySelectedWhenSelected && m_PopulateWithFilesSelected)
            {
                GUI.color = m_DullGreen;
            }


            if (GUILayout.Button(new GUIContent("Play File When Selected: \n" + (m_PlaySelectedWhenSelected && m_PopulateWithFilesSelected ? " ON " : " OFF "), m_PlayFileWhenSelectedTooltip)
                , m_PlaySelectedWhenSelected ? m_ToggleButtonStyleToggled : m_ToggleButtonStyleNormal, GUILayout.MaxWidth(150), GUILayout.MinHeight(40)))
            {
                m_PlaySelectedWhenSelected = !m_PlaySelectedWhenSelected;
            }

            GUI.color = m_DefaultColor;
            GUI.enabled = true;
        }

        private void DrawClip()
        {
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUI.enabled = !m_PopulateWithFilesSelected;
            GUILayout.Label(new GUIContent("File: ", m_ClipFileTooltip));
            m_Sound = EditorGUILayout.ObjectField("", m_Sound, typeof(AudioClip), false, GUILayout.MinWidth(300)) as AudioClip;
            GUI.enabled = true;

            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            string clipName = m_Sound == null ? "[NA]" : m_Sound.name;
            string prefix = m_IsPlaying ? "Playing" : "Queued";
            prefix = m_IsPaused ? "Paused" : prefix;

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            int previousFontsize = GUI.skin.label.fontSize;

            GUI.skin.label.fontSize = 15;
            GUILayout.Label(new GUIContent($"{prefix}: {clipName}", m_ClipPlayingTitleTootip));
            GUI.skin.label.fontSize = previousFontsize;

            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            GUILayout.Label("");
        }

        private void DrawPlaylist()
        {
            //list playlist
            bool showPlaylist = false;
            if (m_SelectedSounds != null)
            {
                if (m_SelectedSounds.Count > 0 && m_PopulateWithFilesSelected)
                    showPlaylist = true;
            }

            if (showPlaylist)
            {
                GUILayout.Label(new GUIContent("Playlist", m_PlaylistTitleTooltip));


                for (int i = 0; i < m_SelectedSounds.Count; i++)
                {
                    DrawPlaylistItem(m_SelectedSounds[i], i);
                }


            }
        }

        private void DrawPlaylistItem(AudioClip clip, int index)
        {
            GUILayout.BeginHorizontal();

            if (GUILayout.Button(m_MediaPlayButtonContex, GUILayout.MaxWidth(20), GUILayout.MaxHeight(20)))
            {
                Stop(true);
                m_CurrentSoundIndex = index;
                Play();
            }

            string prefix = "";

            if (m_SelectedSounds.Count > m_CurrentSoundIndex && clip == m_SelectedSounds[m_CurrentSoundIndex])
                prefix += "›";
            GUILayout.Label(new GUIContent($"{prefix}{clip.name}", m_PlaylistItemTooltip));
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }

        private void OnInspectorUpdate()
        {
            if (m_SelectedSounds == null)
                m_SelectedSounds = new List<AudioClip>();

            if (m_PopulateWithFilesSelected)
            {
                PopulateSelected();
            }

            if (m_CurrentSoundObj != null)
            {
                if (m_NewClipProgress != m_ClipProgress)
                {
                    if (m_NewClipProgress >= m_CurrentAudioSource.clip.length)
                    {
                        Stop();
                        return;
                    }
                    else
                        m_CurrentAudioSource.time = m_NewClipProgress;
                }

                m_CurrentAudioSource.volume = Mathf.Pow(100, -(1 - m_SoundVolume)) - .01f;
                float decimals = m_ClipLength > 1 ? 1 : 10;
                m_ClipProgress = Mathf.RoundToInt(m_CurrentAudioSource.time * decimals) / decimals;
                m_CurrentAudioSource.loop = m_SelectedSounds.Count <= 1 ? m_LoopSelection : false;
                m_NewClipProgress = m_ClipProgress;
            }

            if (m_Sound != m_LastSound)
            {
                Stop();
                Repaint();
                return;
            }

            if (m_CurrentSoundObj == null)
            {
                Stop(true);
                Repaint();
                return;
            }

            if (!m_CurrentAudioSource.isPlaying && !m_IsPaused)
            {
                Stop();
                Repaint();
                return;
            }

            Repaint();
        }

        private void OnDestroy()
        {
            Stop(true);
        }

        private void Update()
        {
            bool enteredPlayMode = Application.isPlaying && !inPlayModeLastFrame;
            bool exitedPlayMode = !Application.isPlaying && inPlayModeLastFrame;

            if (enteredPlayMode)
            {
                m_SoundObjForDestroyWhenExitingPlayMode = m_CurrentSoundObj;
            }
            if (exitedPlayMode && m_SoundObjForDestroyWhenExitingPlayMode != null)
            {
                DestroyImmediate(m_SoundObjForDestroyWhenExitingPlayMode);
                m_SoundObjForDestroyWhenExitingPlayMode = null;
            }

            inPlayModeLastFrame = Application.isPlaying;
        }

        private void Play()
        {
            m_tempHoldOnSoftStop = false;
            m_IsPaused = false;

            if (m_CurrentSoundObj != null)
            {
                Stop(true);
            }

            if (m_Sound != null && m_PopulateWithFilesSelected)
            {
                m_Sound = null;
            }

            if (m_Sound == null && m_SelectedSounds != null)
            {
                if (m_SelectedSounds.Count > 0 && m_SelectedSounds.Count > m_CurrentSoundIndex)
                {
                    m_Sound = m_SelectedSounds[m_CurrentSoundIndex];
                }
                else
                {
                    if (m_SelectedSounds.Count > 0)
                        m_Sound = m_SelectedSounds[0];
                    m_CurrentSoundIndex = 0;
                }
            }

            if (m_Sound != null)
            {
                m_CurrentSoundObj = new GameObject($"AudioPlayer[  {m_Sound.name}  ]");
                m_CurrentAudioSource = m_CurrentSoundObj.AddComponent<AudioSource>();

                if (!Application.isPlaying)
                    m_CurrentSoundObj.AddComponent<DisableOnPlayMode>();

                m_CurrentAudioSource.playOnAwake = true;
                m_CurrentAudioSource.clip = m_Sound;
                m_CurrentAudioSource.loop = m_SelectedSounds.Count <= 1 ? m_LoopSelection : false;
                m_CurrentAudioSource.volume = Mathf.Pow(100, -(1 - m_SoundVolume)) - .01f;
                m_ClipProgress = 0;
                m_CurrentAudioSource.Play();
                m_IsPlaying = true;
                m_LastSound = m_Sound;

                float decimals = m_Sound.length > 1 ? 10 : 100;
                m_ClipLength = Mathf.RoundToInt(m_Sound.length * decimals) / decimals;
            }

        }

        private void Stop(bool hardStop = false)
        {
            if (m_CurrentSoundObj != null)
            {
                DestroyImmediate(m_CurrentSoundObj, true);

            }
            m_CurrentSoundObj = null;
            m_CurrentAudioSource = null;
            m_ClipProgress = 0;
            m_NewClipProgress = 0;
            m_IsPlaying = false;
            m_IsPaused = false;

            if (!m_PopulateWithFilesSelected && m_SelectedSounds != null)
                m_SelectedSounds.Clear();


            if (hardStop || m_tempHoldOnSoftStop)
            {
                m_tempHoldOnSoftStop = true;
                return;
            }

            if (m_SelectedSounds != null)
            {
                if (m_SelectedSounds.Count > 1)
                {
                    if (m_LoopSelection && m_CurrentSoundIndex >= m_SelectedSounds.Count - 1)
                        m_CurrentSoundIndex = -1;

                    if (m_CurrentSoundIndex < m_SelectedSounds.Count - 1)
                    {
                        m_CurrentSoundIndex++;
                        m_Sound = m_SelectedSounds[m_CurrentSoundIndex];

                    }
                }
            }

            if (m_LastSound != m_Sound && m_Sound != null && (m_LoopSelection || m_PlaySelectedWhenSelected || m_SelectedSounds.Count > 1))
            {
                Play();
            }

            m_LastSound = m_Sound;

            if (m_Sound != null)
            {
                float decimals = m_Sound.length > 1 ? 10 : 100;
                m_ClipLength = Mathf.RoundToInt(m_Sound.length * decimals) / decimals;
            }
        }

        private void Pause()
        {
            if (m_CurrentAudioSource == null)
                return;

            if (!m_IsPaused)
            {
                m_CurrentAudioSource.Pause();
                m_IsPaused = true;
            }
            else
            {
                m_CurrentAudioSource.UnPause();
                m_IsPaused = false;
            }
        }

        private void PlayNext()
        {
            if (m_SelectedSounds == null)
                return;
            if (m_SelectedSounds.Count <= 1)
                return;

            bool wasPlaying = m_IsPlaying;
            if (m_CurrentAudioSource != null)
            {
                Stop(true);
            }

            m_CurrentSoundIndex++;
            if (m_CurrentSoundIndex >= m_SelectedSounds.Count)
                m_CurrentSoundIndex = 0;

            m_Sound = m_SelectedSounds[m_CurrentSoundIndex];

            if (wasPlaying)
                Play();
        }

        private void PlayPrevious()
        {
            if (m_SelectedSounds == null)
                return;
            if (m_SelectedSounds.Count <= 1)
                return;

            bool wasPlaying = m_IsPlaying;
            if (m_CurrentAudioSource != null)
            {
                Stop(true);
            }

            m_CurrentSoundIndex--;
            if (m_CurrentSoundIndex < 0)
                m_CurrentSoundIndex = m_SelectedSounds.Count - 1;

            m_Sound = m_SelectedSounds[m_CurrentSoundIndex];

            if (wasPlaying)
                Play();

        }

        private void PopulateSelected()
        {
            Object[] selectedObjects = Selection.objects;
            if (selectedObjects.Length > 0)
            {
                List<AudioClip> selected = new List<AudioClip>();
                for (int i = 0; i < selectedObjects.Length; i++)
                {
                    if (selectedObjects[i].GetType() == typeof(AudioClip))
                    {
                        selected.Add((AudioClip)selectedObjects[i]);
                    }
                }

                //check if the selected clips list contains the same clips up to the current index
                if (selected.Count > m_CurrentSoundIndex && m_SelectedSounds.Count > m_CurrentSoundIndex)
                {
                    List<AudioClip> selectedTrimmed = selected.GetRange(0, m_CurrentSoundIndex + 1);
                    List<AudioClip> currentTrimmed = m_SelectedSounds.GetRange(0, m_CurrentSoundIndex + 1);


                    if (!selectedTrimmed.ContentsAreEqual(currentTrimmed))
                    {
                        m_CurrentSoundIndex = 0;
                    }
                }
                else
                {
                    m_CurrentSoundIndex = 0;
                }

                if (!selected.ContentsAreEqual(m_SelectedSounds))
                {
                    m_SelectedSounds = selected;

                    if (m_PlaySelectedWhenSelected && m_PopulateWithFilesSelected && m_Sound != m_SelectedSounds[m_CurrentSoundIndex])
                    {
                        Play();
                    }
                }

            }


        }
    }
}
