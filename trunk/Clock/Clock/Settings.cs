//////////////////////////////////////////////////////////////////////////////////////////////////////
//                                  Copyright Adrian Vinca 2008                                     //
//////////////////////////////////////////////////////////////////////////////////////////////////////
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace Clock
{
    /// <summary>
    /// The application settings.
    /// </summary>
    public class Settings
    {
        public Settings()
        {
            this.offset = new TimeSpan();
        }

        private TimeSpan offset;
        
        /// <summary>
        /// Offset from the device time. We serialize the string representation of the Offset.
        /// </summary>
        [XmlIgnore]
        public TimeSpan Offset
        {
            get
            {
                return this.offset;
            }
            set
            {
                this.offset = value;
            }
        }

        /// <summary>
        /// We serialize the Offset as a string. It seems that XmlSerializer doesn't like TimeSpan.
        /// </summary>
        public string OffsetString
        {
            get
            {
                return this.offset.ToString();
            }
            set
            {
                this.offset = TimeSpan.Parse(value);
            }
        }

        /// <summary>
        /// The Background.
        /// </summary>
        public string Background
        {
            get;
            set;
        }

        public bool Shuffle
        {
            get;
            set;
        }

        public bool AlbumArt
        {
            get;
            set;
        }

        // Timer
        public bool Timer
        {
            get;
            set;
        }
    }
}