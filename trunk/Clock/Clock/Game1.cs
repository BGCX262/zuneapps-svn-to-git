//////////////////////////////////////////////////////////////////////////////////////////////////////
//                                  Copyright Adrian Vinca 2008                                     //
//////////////////////////////////////////////////////////////////////////////////////////////////////
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Storage;
using Microsoft.Xna.Framework.GamerServices;
using System.IO;
using System.Xml.Serialization;
using System.Reflection;
using System.Text;

namespace Clock
{
    /// <summary>
    /// Items which can be set. The application cycles through them.
    /// </summary>
    enum SettingItem
    {
        Timer,
        Playlist,
        Background,
        Album_Art,
        Shuffle,
        Hours,
        Minutes,
        Seconds,
        Days,
        //Month,
        //Year,

        /// <summary>
        /// We use this value to count the items in the enum. It has to be the last value in the enum.
        /// </summary>
        Last
    }

    /// <summary>
    /// The setting category.
    /// </summary>
    enum SettingCategory
    {
        /// <summary>
        /// The category for the Timer.
        /// </summary>
        Timer,

        /// <summary>
        /// The category for the Album art.
        /// </summary>
        AlbumArt,

        /// <summary>
        /// The background category.
        /// </summary>
        Background,

        /// <summary>
        /// The playlist category.
        /// </summary>
        Playlist,

        /// <summary>
        /// The shuffle category
        /// </summary>
        Shuffle,

        /// <summary>
        /// The Time category.
        /// </summary>
        Time,

    }

    enum TimerControl
    {
        Reset,
        Start,
        Pause,

        Last
    }

    /// <summary>
    /// Represents the direction of the adjustment.
    /// </summary>
    enum SettingAdjustmentDirection
    {
        Increment,
        Decrement
    }

    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        SpriteFont spriteFontSmall;
        SpriteFont spriteFontLarge;
        Vector2 timePosition;
        Vector2 datePosition;
        Vector2 versionPosition;
        Vector2 settingMessagePosition;
        Vector2 settingValuePosition;
        Rectangle backgroundRectangle;

        // Timer code
        TimerControl currentlySelectedTimerControl;
        Vector2 timerPosition;
        TimeSpan timer;

        Texture2D background;

        Settings settings;

        GamePadState currentState;
        bool isInSettingMode;
        SettingItem ItemBeingSet;

        string version;

        # region fields available only while in setting mode
        MediaLibrary mediaLibrary;
        PictureCollection pictures;
        int currentlySelectedPictureIndex = -1;
        #endregion

        PlaylistCollection playlists;
        int currentlySelectedPlaylistIndex = -1;

        string previousSong = "";
        Texture2D currentAlbumArt = null;
        Texture2D CurrentAlbumArt
        {
            set
            {
                if (currentAlbumArt != null && !currentAlbumArt.IsDisposed)
                {
                    currentAlbumArt.Dispose();
                }

                this.currentAlbumArt = value;
            }
            get
            {
                return this.currentAlbumArt;
            }
        }

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            this.GetDevice();
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            // TODO: use this.Content to load your game content here
            spriteFontSmall = this.Content.Load<SpriteFont>("SpriteFontSmall");
            spriteFontLarge = this.Content.Load<SpriteFont>("SpriteFontLarge");

            // Timer addon code
            timerPosition = new Vector2(graphics.GraphicsDevice.Viewport.Width / 2, 20);
            timer = new TimeSpan();
            
            

            timePosition = new Vector2(graphics.GraphicsDevice.Viewport.Width / 2, graphics.GraphicsDevice.Viewport.Height / 2 - 15);
            datePosition = new Vector2(graphics.GraphicsDevice.Viewport.Width / 2, graphics.GraphicsDevice.Viewport.Height / 2 + 15);
            settingMessagePosition = new Vector2(graphics.GraphicsDevice.Viewport.Width / 2, graphics.GraphicsDevice.Viewport.Height / 4 * 3);
            settingValuePosition = new Vector2(graphics.GraphicsDevice.Viewport.Width / 2, graphics.GraphicsDevice.Viewport.Height / 4 * 3 + 15);
            versionPosition = new Vector2(graphics.GraphicsDevice.Viewport.Width / 2, graphics.GraphicsDevice.Viewport.Height / 4);

            this.LoadSettings(this.device);

            // I dont want the timer running
            this.settings.Timer = false;

            this.version = "Zune Clock " + this.GetType().Assembly.GetName().Version.ToString();
            this.backgroundRectangle = new Rectangle(0, 0, graphics.GraphicsDevice.Viewport.Width, graphics.GraphicsDevice.Viewport.Height);

            this.InitializeMediaLibrary();

            this.UpdateBackgroundPictureResource();

            // Performance: 6 times slower than by default.
            this.TargetElapsedTime = TimeSpan.FromMilliseconds(100);
            MediaPlayer.IsShuffled = this.settings.Shuffle;
        }

        /// <summary>
        /// Initializes the media library.
        /// </summary>
        private void InitializeMediaLibrary()
        {
            this.mediaLibrary = new MediaLibrary();
            this.pictures = mediaLibrary.Pictures;

            int currentIndex = 0;

            // Search for the picture specified in the settings file.
            // Store the pictures into a list, so that we can easily navigate back and forward (the collection allows only forward navigation).
            foreach (Picture picture in this.pictures)
            {
                if ((this.settings.Background != null) && (this.settings.Background != "") && (picture.Name == this.settings.Background))
                {
                    this.currentlySelectedPictureIndex = currentIndex;
                }

                currentIndex++;
            }

            this.playlists = this.mediaLibrary.Playlists;

        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
            {
                if (this.isInSettingMode)
                {
                    this.EndSettingMode();
                }
                else
                {
                    this.Exit();
                }
            }

            // TODO: Add your update logic here
            // Update the timer if its running
            if (this.settings.Timer)
            {
                this.timer = this.timer.Add(gameTime.ElapsedGameTime);
            }

            GamePadState previousState = currentState;
            this.currentState = GamePad.GetState(PlayerIndex.One);

            if (currentState.Buttons.A == ButtonState.Pressed && previousState.Buttons.A == ButtonState.Released)
            {
                if (this.isInSettingMode)
                {
                    // this.EndSettingMode();
                }
                else
                {
                    this.BeginSettingMode();
                    base.Update(gameTime);
                    return;
                }
            }

            if (this.isInSettingMode)
            {
                if (currentState.DPad.Up == ButtonState.Pressed && previousState.DPad.Up == ButtonState.Released)
                {
                    this.AdjustSetting(SettingAdjustmentDirection.Increment);
                }

                if (currentState.DPad.Down == ButtonState.Pressed && previousState.DPad.Down == ButtonState.Released)
                {
                    this.AdjustSetting(SettingAdjustmentDirection.Decrement);
                }

                if (currentState.DPad.Right == ButtonState.Pressed && previousState.DPad.Right == ButtonState.Released)
                {
                    this.NextSetting();
                }

                if (currentState.DPad.Left == ButtonState.Pressed && previousState.DPad.Left == ButtonState.Released)
                {
                    this.PreviousSetting();
                }

                if (currentState.Buttons.A == ButtonState.Pressed && previousState.Buttons.A == ButtonState.Released)
                {
                    SettingCategory currentSettingCategory = GetSettingCategory(this.ItemBeingSet);

                    switch (currentSettingCategory)
                    {
                        case SettingCategory.Time:
                            this.settings.Offset = new TimeSpan();
                            break;
                        case SettingCategory.Background:
                            this.settings.Background = "";
                            this.UpdateBackgroundPictureResource();
                            break;
                        case SettingCategory.Timer:
                            switch (this.currentlySelectedTimerControl)
                            {
                                case TimerControl.Start:
                                    this.settings.Timer = true;
                                    break;
                                    // Else: Fall through down to reset, avoid code duplication
                                case TimerControl.Reset:
                                    this.timer = new TimeSpan(0);
                                    break;
                                case TimerControl.Pause:
                                    this.settings.Timer = false;
                                    break;
                            }
                            break;
                        case SettingCategory.Playlist:
                            if (this.currentlySelectedPlaylistIndex != -1)
                            {
                                if (this.playlists[this.currentlySelectedPlaylistIndex].Songs.Count > 0)
                                {
                                    MediaPlayer.Play(this.playlists[this.currentlySelectedPlaylistIndex].Songs);
                                }
                            }
                            else
                            {
                                MediaPlayer.Stop();
                            }
                            break;
                    }
                }
            }

            if (!this.isInSettingMode)
            {
                if (currentState.Buttons.B == ButtonState.Pressed && previousState.Buttons.B == ButtonState.Released)
                {
                    if (MediaPlayer.State == MediaState.Playing)
                    {
                        MediaPlayer.Pause();
                    }
                    else if (MediaPlayer.State == MediaState.Paused)
                    {
                        MediaPlayer.Resume();
                    }
                }

                if (currentState.DPad.Up == ButtonState.Pressed && previousState.DPad.Up == ButtonState.Released)
                {
                    float volume = MediaPlayer.Volume + (float)0.1;
                    if (volume > 1)
                    {
                        volume = 1;
                    }

                    MediaPlayer.Volume = volume;
                }

                if (currentState.DPad.Down == ButtonState.Pressed && previousState.DPad.Down == ButtonState.Released)
                {
                    float volume = MediaPlayer.Volume - (float)0.1;
                    if (volume > 1)
                    {
                        volume = 1;
                    }

                    MediaPlayer.Volume = volume;   
                }

                if (currentState.DPad.Right == ButtonState.Pressed && previousState.DPad.Right == ButtonState.Released)
                {
                    MediaPlayer.Queue.MoveNext();
                }

                if (currentState.DPad.Left == ButtonState.Pressed && previousState.DPad.Left == ButtonState.Released)
                {
                    MediaPlayer.Queue.MovePrevious();
                }
            }

            // Verify if the album name is different to update the album art.
            if ((MediaPlayer.Queue.ActiveSong != null) && (MediaPlayer.Queue.ActiveSong.Name != previousSong))
            {
                this.CurrentAlbumArt = MediaPlayer.Queue.ActiveSong.Album.GetAlbumArt(this.Services);

                if (this.CurrentAlbumArt != null && this.CurrentAlbumArt.IsDisposed)
                {

                }

                previousSong = MediaPlayer.Queue.ActiveSong.Name;
            }

            base.Update(gameTime);
        }

        /// <summary>
        /// Adjusts the current setting in the direction specified.
        /// </summary>
        /// <param name="adjustmentDirection">The adjustment direction for the current setting.</param>
        private void AdjustSetting(SettingAdjustmentDirection adjustmentDirection)
        {
            if (!this.isInSettingMode)
            {
                throw new InvalidOperationException("Attempting to adjust a setting while not in setting mode");
            }

            SettingCategory currentSettingCategory = GetSettingCategory(this.ItemBeingSet);

            switch (currentSettingCategory)
            {
                case SettingCategory.Time:
                    TimeSpan timeSpan = CalculateTimeIncrement(this.ItemBeingSet);
                    if (adjustmentDirection == SettingAdjustmentDirection.Increment)
                    {
                        this.settings.Offset = this.settings.Offset.Add(timeSpan);
                    }
                    else
                    {
                        this.settings.Offset = this.settings.Offset.Subtract(timeSpan);
                    }
                    break;
                case SettingCategory.Shuffle:
                    this.settings.Shuffle = !this.settings.Shuffle;
                    MediaPlayer.IsShuffled = this.settings.Shuffle;

                    break;
                case SettingCategory.AlbumArt:
                    this.settings.AlbumArt = !this.settings.AlbumArt;

                    break;
                // Timer handling
                case SettingCategory.Timer:
                    if (this.ItemBeingSet == SettingItem.Timer)
                    {
                        if (adjustmentDirection == SettingAdjustmentDirection.Increment)
                        {
                            this.currentlySelectedTimerControl++;
                            if (this.currentlySelectedTimerControl == TimerControl.Last)
                            {
                                this.currentlySelectedTimerControl = TimerControl.Reset;
                            }
                        }
                        else
                        {
                            if (this.currentlySelectedTimerControl <= TimerControl.Reset)
                            {
                                this.currentlySelectedTimerControl = TimerControl.Pause;
                            } 
                            else
                            {
                                this.currentlySelectedTimerControl--;
                            }
                            

                        }
                    }
                    break;
                case SettingCategory.Playlist:
                    if (this.playlists.Count > 0)
                    {
                        if (this.ItemBeingSet == SettingItem.Playlist)
                        {
                            if (adjustmentDirection == SettingAdjustmentDirection.Increment)
                            {
                                this.currentlySelectedPlaylistIndex++;
                                if (this.currentlySelectedPlaylistIndex >= this.playlists.Count)
                                {
                                    this.currentlySelectedPlaylistIndex = -1;
                                }
                            }
                            else
                            {
                                this.currentlySelectedPlaylistIndex--;
                                if (this.currentlySelectedPlaylistIndex < -1)
                                {
                                    this.currentlySelectedPlaylistIndex = this.playlists.Count - 1;
                                }
                            }
                        }
                    }
                    break;

                case SettingCategory.Background:
                    if (this.pictures.Count > 0)
                    {
                        if (this.ItemBeingSet == SettingItem.Background)
                        {
                            if (adjustmentDirection == SettingAdjustmentDirection.Increment)
                            {
                                this.currentlySelectedPictureIndex++;
                                if (this.currentlySelectedPictureIndex >= this.pictures.Count)
                                {
                                    this.currentlySelectedPictureIndex = -1;
                                }
                            }
                            else
                            {
                                this.currentlySelectedPictureIndex--;
                                if (this.currentlySelectedPictureIndex < -1)
                                {
                                    this.currentlySelectedPictureIndex = this.pictures.Count - 1;
                                }
                            }

                            if (this.currentlySelectedPictureIndex == -1)
                            {
                                this.settings.Background = "";
                            }
                            else
                            {
                                this.settings.Background = pictures[currentlySelectedPictureIndex].Name;
                            }
                        }
                        else
                        {
                            this.settings.Background = "";
                            this.currentlySelectedPictureIndex = -1;
                        }
                    }

                    this.UpdateBackgroundPictureResource();
                    break;
            }
        }

        /// <summary>
        /// Updates the texture for the background image based on the new settings.
        /// </summary>
        private void UpdateBackgroundPictureResource()
        {
            if (this.background != null)
            {
                // Dispose the previous background so that we don't run out of memory.
                this.background.Dispose();
                this.background = null;
                // GC.Collect();
            }

            if (this.settings.Background == "")
            {
                this.currentlySelectedPictureIndex = -1;
            }

            if (this.currentlySelectedPictureIndex == -1)
            {
                this.settings.Background = "";
            }
            
            if (this.currentlySelectedPictureIndex != -1)
            {
                this.background = pictures[currentlySelectedPictureIndex].GetTexture(this.Services);

                // If the texture was disposed previously, GetTexture returns a texture which was disposed and we can't use it.
                // This happens if you navigate to a picture selected before.
                // The only way I found to refresh is to get reinitialize the list of Pictures. 
                // Not the most elegant solution, but at least it shouldn't happen that often (only when changing the background in the setting mode and only if you reach a Picture you've seen already.
                if (this.background.IsDisposed)
                {
                    this.InitializeMediaLibrary();
                    // GC.Collect();
                    this.background = pictures[currentlySelectedPictureIndex].GetTexture(this.Services);
                }
            }
            
        }

        /// <summary>
        /// Begins setting mode.
        /// </summary>
        private void BeginSettingMode()
        {
            this.isInSettingMode = true;
            this.ItemBeingSet = SettingItem.Timer;
        }

        /// <summary>
        /// Ends setting mode.
        /// </summary>
        private void EndSettingMode()
        {
            this.isInSettingMode = false;
            this.SaveData();
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            graphics.GraphicsDevice.Clear(Color.Black);

            // TODO: Add your drawing code here
            spriteBatch.Begin();
            if (this.background != null && !this.background.IsDisposed)
            {
                Color tint = this.isInSettingMode ? Color.Gray : Color.White;
                spriteBatch.Draw(this.background, this.backgroundRectangle, tint);
            }

            if (this.CurrentAlbumArt != null && !this.isInSettingMode && this.settings.AlbumArt)
            {
                // 40 = (320 - 240) / 2
                // Rectangle albumArtRectangle = new Rectangle(0, 40, 240, 240);

                Rectangle albumArtRectangle = new Rectangle(60, 5, 120, 120);

                spriteBatch.Draw(this.CurrentAlbumArt, albumArtRectangle, Color.White);
            }

            //Draw the date and time
            DateTime dateTimeAdjusted = DateTime.Now + this.settings.Offset;

            string timer = string.Format("{0:D2}:{1:D2}:{2:D2}", (long)this.timer.Hours, (long)this.timer.Minutes, (long)this.timer.Seconds);
            string time = dateTimeAdjusted.ToLongTimeString();
            string date = dateTimeAdjusted.ToLongDateString();
            // Find the center of the string
            Vector2 fontOriginTimer = this.spriteFontLarge.MeasureString(timer) / 2;
            Vector2 fontOriginTime = this.spriteFontLarge.MeasureString(time) / 2;
            Vector2 fontOriginDate = this.spriteFontSmall.MeasureString(date) / 2;

            spriteBatch.DrawString(spriteFontLarge, timer, this.timerPosition, Color.White, 0, fontOriginTimer, 1.0f, SpriteEffects.None, 0.5f);
            spriteBatch.DrawString(spriteFontLarge, time, this.timePosition, Color.White, 0, fontOriginTime, 1.0f, SpriteEffects.None, 0.5f);
            spriteBatch.DrawString(spriteFontSmall, date, this.datePosition , Color.White, 0, fontOriginDate, 1.0f, SpriteEffects.None, 0.5f);

            if (this.isInSettingMode)
            {
                StringBuilder settingMessageStringBuilder = new StringBuilder();
                settingMessageStringBuilder.Append("Setting ");
                settingMessageStringBuilder.Append(this.ItemBeingSet.ToString().Replace('_', ' '));

                if (this.ItemBeingSet == SettingItem.Background)
                {
                    if (this.currentlySelectedPictureIndex != -1)
                    {
                        settingMessageStringBuilder.Append(string.Format(" ({0}/{1})", this.currentlySelectedPictureIndex + 1, this.pictures.Count));
                    }
                }

                if (this.ItemBeingSet == SettingItem.Playlist)
                {
                    if (this.currentlySelectedPlaylistIndex != -1)
                    {
                        settingMessageStringBuilder.Append(string.Format(" ({0}/{1})", this.currentlySelectedPlaylistIndex + 1, this.playlists.Count));
                    }
                }

                string settingMessage = settingMessageStringBuilder.ToString();
                Vector2 fontOriginNowSetting = this.spriteFontSmall.MeasureString(settingMessage) / 2;

                spriteBatch.DrawString(spriteFontSmall, settingMessage, this.settingMessagePosition, Color.White, 0, fontOriginNowSetting, 1.0f, SpriteEffects.None, 0.5f);

                Vector2 fontOriginVersion = this.spriteFontSmall.MeasureString(version) / 2;
                spriteBatch.DrawString(spriteFontSmall, version, versionPosition, Color.White, 0, fontOriginVersion, 1.0f, SpriteEffects.None, 0.5f);

                // Calculate the selected setting value
                string selectedSettingValue = null;
                switch (this.ItemBeingSet)
                {
                    case SettingItem.Background:
                        string backgroundName = "No pictures found!";

                        if (this.currentlySelectedPictureIndex == -1)
                        {
                            backgroundName = "No background";
                        }
                        else
                        {
                            if (this.pictures.Count > 0)
                            {
                                backgroundName = this.pictures[this.currentlySelectedPictureIndex].Name;
                            }
                        }

                        selectedSettingValue = NormalizeString(backgroundName);
                        break;
                    case SettingItem.Timer:
                        selectedSettingValue = NormalizeString(this.currentlySelectedTimerControl.ToString());
                        break;
                    case SettingItem.Playlist:
                        string playlistName = "No playlists found!";

                        if (this.currentlySelectedPlaylistIndex == -1)
                        {
                            playlistName = "No playlist";
                        }
                        else
                        {
                            if (this.playlists.Count > 0)
                            {
                                playlistName = String.Format("{0} ({1})", this.playlists[this.currentlySelectedPlaylistIndex].Name, this.playlists[this.currentlySelectedPlaylistIndex].Songs.Count);
                            }
                        }
                        selectedSettingValue = NormalizeString(playlistName);
                        break;

                    case SettingItem.Shuffle:
                        selectedSettingValue = this.settings.Shuffle ? "On" : "Off";
                        break;

                    case SettingItem.Album_Art:
                        selectedSettingValue = this.settings.AlbumArt ? "On" : "Off";
                        break;
                }

                if (selectedSettingValue != null)
                {
                    Vector2 origin = this.spriteFontSmall.MeasureString(selectedSettingValue) / 2;
                    spriteBatch.DrawString(spriteFontSmall, selectedSettingValue, settingValuePosition, Color.White, 0, origin, 1.0f, SpriteEffects.None, 0.5f);
                }

            }

            // Display the song information
            Song activeSong = MediaPlayer.Queue.ActiveSong;
            if (activeSong != null && !this.isInSettingMode)
            {
                string artist = NormalizeString(activeSong.Artist.Name);
                string songName = NormalizeString(activeSong.Name);

                Vector2 artistOrigin = this.spriteFontSmall.MeasureString(artist) / 2;
                Vector2 songNameOrigin = this.spriteFontSmall.MeasureString(songName) / 2;

                // For now, reuse the positions calculated for displaying the settings :)
                spriteBatch.DrawString(spriteFontSmall, artist, this.settingMessagePosition, Color.White, 0, artistOrigin, 1.0f, SpriteEffects.None, 0.5f);
                spriteBatch.DrawString(spriteFontSmall, songName, this.settingValuePosition, Color.White, 0, songNameOrigin, 1.0f, SpriteEffects.None, 0.5f);
            }

            base.Draw(gameTime);
            spriteBatch.End();
        }

        /// <summary>
        /// Normalizes the string so that it doesn't have any characters which go outside the range of characters included in the sprite font.
        /// </summary>
        /// <param name="input">The string to normalize.</param>
        /// <returns>Normalized string.</returns>
        private static string NormalizeString(string input)
        {
            StringBuilder stringBuilder = new StringBuilder(input.Length);
            foreach (char character in input)
            {
                if ((int)character < 32 || (int)character > 126)
                {
                    stringBuilder.Append('_');
                }
                else
                {
                    stringBuilder.Append(character);
                }
            }

            return stringBuilder.ToString();
        }

        /// <summary>
        /// Calculates the TimeSpan corresponding to the item being set.
        /// </summary>
        /// <param name="itemBeingSet">The item being set.</param>
        /// <returns>The timespan.</returns>
        private static TimeSpan CalculateTimeIncrement(SettingItem itemBeingSet)
        {
            TimeSpan timeSpan = new TimeSpan();

            switch (itemBeingSet)
            {
                case SettingItem.Hours:
                    timeSpan = new TimeSpan(1, 0, 0);
                    break;
                case SettingItem.Minutes:
                    timeSpan = new TimeSpan(0, 1, 0);
                    break;
                case SettingItem.Seconds:
                    timeSpan = new TimeSpan(0, 0, 1);
                    break;
                case SettingItem.Days:
                    timeSpan = new TimeSpan(1, 0, 0, 0);
                    break;
            }

            return timeSpan;
        }

        /// <summary>
        /// Cycles to the next setting.
        /// </summary>
        private void NextSetting()
        {
            int item = (int)this.ItemBeingSet;
            item++;
            if (item > (int)SettingItem.Last - 1)
            {
                item = 0;
            }
            this.ItemBeingSet = (SettingItem)item;
        }

        /// <summary>
        /// Cycles to the previous setting.
        /// </summary>
        private void PreviousSetting()
        {
            int item = (int)this.ItemBeingSet;
            item--;
            if (item < 0)
            {
                item = (int)SettingItem.Last - 1;
            }
            this.ItemBeingSet = (SettingItem)item;
        }

        /// <summary>
        /// Saves the data.
        /// </summary>
        private void SaveData()
        {
            this.SaveSettings(this.device);
        }

        Object stateobj;
        private void GetDevice()
        {
            if (!Guide.IsVisible)
            {
                // Reset the device
                device = null;
                stateobj = (Object)"GetDevice for Player One";
                Guide.BeginShowStorageDeviceSelector(
                                PlayerIndex.One, this.GetDeviceAsync, stateobj);
            }
        }

        StorageDevice device;
        void GetDeviceAsync(IAsyncResult result)
        {
            device = Guide.EndShowStorageDeviceSelector(result);
        }

        /// <summary>
        /// Saves the application settings to the specified device.
        /// </summary>
        /// <param name="device">The device to use.</param>
        private void SaveSettings(StorageDevice device)
        {
            // Open a storage container.
            StorageContainer container = device.OpenContainer("Clock");

            // Get the path of the save game.
            string filename = Path.Combine(container.Path, "settings.xml");

            // Open the file, creating it if necessary.
            FileStream stream = File.Open(filename, FileMode.Create);

            // Convert the object to XML data and put it in the stream.
            XmlSerializer serializer = new XmlSerializer(typeof(Settings));
            serializer.Serialize(stream, this.settings);

            // Close the file.
            stream.Close();

            // Dispose the container, to commit changes.
            container.Dispose();
        }

        /// <summary>
        /// Loads the settings from the specified device.
        /// </summary>
        /// <param name="device">The device.</param>
        private void LoadSettings(StorageDevice device)
        {
            // Open a storage container.
            StorageContainer container = device.OpenContainer("Clock");

            // Get the path of the save game.
            string filename = Path.Combine(container.Path, "settings.xml");

            if (File.Exists(filename))
            {
#if DEBUG
                if (System.Diagnostics.Debugger.IsAttached)
                {
                    using (TextReader tr = new StreamReader(filename))
                    {
                        string xml = tr.ReadToEnd();
                        System.Diagnostics.Trace.WriteLine(xml);
                    }
                }
#endif
                try
                {
                    // Open the file
                    FileStream stream = File.Open(filename, FileMode.Open);

                    // Convert the object to XML data and put it in the stream.
                    XmlSerializer serializer = new XmlSerializer(typeof(Settings));
                    this.settings = serializer.Deserialize(stream) as Settings;

                    // Close the file.
                    stream.Close();
                }
                catch (Exception exception)
                {
                    // In some cases, XmlSerializer throws exception when loading the data. We try to fix the problem by deleting the file.
                    // Hopefully this issue should be fixed now.
                    System.Diagnostics.Trace.WriteLine(exception.ToString());
                    
                    File.Delete(filename);

                    // As a preliminary way to signal that we didn't load the settings, for now we go into setting mode.
                    this.BeginSettingMode();
                }
            }

            // Dispose the container, to commit changes.
            container.Dispose();

            if (this.settings == null)
            {
                this.settings = new Settings();
            }
        }

        /// <summary>
        /// Gets the category for the specified setting.
        /// </summary>
        /// <param name="settingItem">The setting item for which we want the category.</param>
        /// <returns>.</returns>
        private static SettingCategory GetSettingCategory(SettingItem settingItem)
        {
            switch (settingItem)
            {
                case SettingItem.Hours:
                case SettingItem.Minutes:
                case SettingItem.Seconds:
                case SettingItem.Days:
                    return SettingCategory.Time;
                case SettingItem.Timer:
                    return SettingCategory.Timer;
                case SettingItem.Background:
                    return SettingCategory.Background;
                case SettingItem.Playlist:
                    return SettingCategory.Playlist;
                case SettingItem.Shuffle:
                    return SettingCategory.Shuffle;
                case SettingItem.Album_Art:
                    return SettingCategory.AlbumArt;
                default:
                    throw new InvalidOperationException("Unknown category for the specified setting");
            }
        }
    }
}
