﻿/* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
 *  .Net Core Plugin Manager is distributed under the GNU General Public License version 3 and  
 *  is also available under alternative licenses negotiated directly with Simon Carter.  
 *  If you obtained Service Manager under the GPL, then the GPL applies to all loadable 
 *  Service Manager modules used on your system as well. The GPL (version 3) is 
 *  available at https://opensource.org/licenses/GPL-3.0
 *
 *  This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY;
 *  without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 *  See the GNU General Public License for more details.
 *
 *  The Original Code was created by Simon Carter (s1cart3r@gmail.com)
 *
 *  Copyright (c) 2019 Simon Carter.  All Rights Reserved.
 *
 *  Product:  SharedPluginFeatues
 *  
 *  File: IDynamicContentTemplate.cs
 *
 *  Purpose: Template for dynamic content item
 *
 *  Date        Name                Reason
 *  22/11/2020  Simon Carter        Initially Created
 *
 * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * */

using System;
using System.Text;

namespace SharedPluginFeatures.DynamicContent
{
    /// <summary>
    /// Abstract class for implementing a dyncamic content template item
    /// </summary>
    public abstract class DynamicContentTemplate
    {
        #region Private Members

        private const int DefaultPixelWidth = 800;
        private const int MaximumColumnCount = 12;
        private const int MinimumColumnCount = 1;
        private const int PercentPerColumn = 100 / MaximumColumnCount;
        private const int PixelsPerColumn = DefaultPixelWidth / MaximumColumnCount;

        private const string InvalidUniqueIdValueNull = "value can not be null or empty";
        private const string InvalidUniqueIdValueChars = "Id can only contain characters 0 to 9, a to Z and -";
        private const string ValidIdCharacters = "0123456789abcdefghijklmnopqrstuvwxyz-ABCDEFGHIJKLMNOPQRSTUVWXYZ";

        private string _uniqueId;

        #endregion Private Members

        #region Properties

        /// <summary>
        /// Unique id for this template item
        /// 
        /// If no user defined id is specified, this will return a new guid
        /// </summary>
        /// <value>string</value>
        /// <exception cref="InvalidOperationException">When setting the property with a null or empty unique id</exception>
        /// <exception cref="InvalidOperationException">If the unique id contians characters other than a to Z, dash (-) and 0 to 9</exception>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Globalization", "CA1303:Do not pass literals as localized parameters", Justification = "Not required to be localized")]
        public string UniqueId
        {
            get
            {
                if (String.IsNullOrEmpty(_uniqueId))
                {
                    _uniqueId = Guid.NewGuid().ToString();
                }

                return _uniqueId;
            }

            set
            {
                if (String.IsNullOrEmpty(value))
                {
                    throw new InvalidOperationException(InvalidUniqueIdValueNull);
                }

                bool validChars = true;

                foreach (char c in value)
                {
                    if (!ValidIdCharacters.Contains(c.ToString()))
                    {
                        validChars = false;
                        break;
                    }
                }

                if (!validChars)
                {
                    throw new InvalidOperationException(InvalidUniqueIdValueChars);
                }

                _uniqueId = value;
            }
        }

        /// <summary>
        /// Returns the number of columns that would be used to display the control if using bootstrap or other column based framework
        /// </summary>
        /// <value>int</value>
        public int ColumnCount
        {
            get
            {
                if (WidthType == DynamicContentWidthType.Percentage)
                {
                    return Shared.Utilities.CheckMinMax(Width / PercentPerColumn, MinimumColumnCount, MaximumColumnCount);
                }

                if (WidthType == DynamicContentWidthType.Pixels)
                {
                    return Shared.Utilities.CheckMinMax(Width / PixelsPerColumn, MinimumColumnCount, MaximumColumnCount);
                }

                return Shared.Utilities.CheckMinMax(Width, MinimumColumnCount, MaximumColumnCount);
            }
        }

        #endregion Properties

        #region Abstract Properties

        /// <summary>
        /// Gets the underlying assembly qualified name
        /// </summary>
        public abstract string AssemblyQualifiedName { get; }

        /// <summary>
        /// Name of the partial view used to edit the template item.
        /// </summary>
        /// <value>string</value>
        /// <exception cref="InvalidOperationException">If the value returned is null, empty or a duplicate</exception>
        public abstract string EditorAction { get; }

        /// <summary>
        /// Name of dynamic content template
        /// </summary>
        /// <value>string</value>
        public abstract string Name { get; }

        /// <summary>
        /// The sort order of the control relative to others when being displayed
        /// </summary>
        /// <value>int</value>
        public abstract int SortOrder { get; set; }

        /// <summary>
        /// Specifies the content width type
        /// </summary>
        public abstract DynamicContentWidthType WidthType { get; set; }

        /// <summary>
        /// Width of the control when laid out, this is also dependent on <seealso cref="WidthType"/>
        /// </summary>
        /// <value>int</value>
        public abstract int Width { get; set; }

        /// <summary>
        /// Defines the Height type
        /// </summary>
        public abstract DynamicContentHeightType HeightType { get; set; }

        /// <summary>
        /// Height of the control when laid out, this is also dependent on <seealso cref="HeightType"/>
        /// </summary>
        /// <value>int</value>
        public abstract int Height { get; set; }

        /// <summary>
        /// The data provided by the dynamic content template
        /// </summary>
        /// <value>string</value>
        public abstract string Data { get; set; }

        /// <summary>
        /// The date/time the dynamic content is active from
        /// </summary>
        /// <value>DateTime</value>
        public abstract DateTime ActiveFrom { get; set; }

        /// <summary>
        /// The date/time the dynamic content ceases to be active
        /// </summary>
        public abstract DateTime ActiveTo { get; set; }

        #endregion Abstract Properties

        #region Abstract Methods

        /// <summary>
        /// Retrieves the content in a manner that can be displayed within the UI
        /// </summary>
        /// <value>string</value>
        public abstract string Content();

        #endregion Abstract Methods

        #region Pretected Methods

        /// <summary>
        /// Generates the html used as start of the custom block, taking into account the width/height types
        /// </summary>
        /// <param name="stringBuilder">StringBuilder instance where start block will be appended to.</param>
        /// <exception cref="ArgumentNullException">Raised if stringBuilder parameter is null</exception>
        protected void HtmlStart(StringBuilder stringBuilder)
        {
            if (stringBuilder == null)
            {
                throw new ArgumentNullException(nameof(stringBuilder));
            }

            if (WidthType == DynamicContentWidthType.Columns)
            {
                stringBuilder.AppendFormat("<div class=\"col-sm-{0}\"", ColumnCount);

                if (HeightType != DynamicContentHeightType.Automatic)
                {
                    stringBuilder.AppendFormat(" style=\"{0}\"", GetHeight());
                }

                stringBuilder.Append(">");
            }
            else if (WidthType == DynamicContentWidthType.Percentage)
            {
                stringBuilder.AppendFormat("<div style=\"width:{0}% !important;{1};display:block;\" />", Width, GetHeight());
            }
            else if (WidthType == DynamicContentWidthType.Pixels)
            {
                stringBuilder.AppendFormat("<div style=\"width:{0}px !important;{1};display:block;\" />", Width, GetHeight());
            }
        }

        /// <summary>
        /// Generates the end of the html block taking into account the width/height types
        /// </summary>
        /// <param name="stringBuilder">StringBuilder instance where end block will be appended to.</param>
        /// <exception cref="ArgumentNullException">Raised if stringBuilder parameter is null</exception>
        protected void HtmlEnd(StringBuilder stringBuilder)
        {
            if (stringBuilder == null)
            {
                throw new ArgumentNullException(nameof(stringBuilder));
            }

            if (WidthType == DynamicContentWidthType.Columns)
            {
                stringBuilder.Append("</div>");
            }
        }

        #endregion Protected Methods

        #region Private Methods

        private string GetHeight()
        {
            if (HeightType == DynamicContentHeightType.Percentage)
            {
                return String.Format("height:{0}% !important;", Height);
            }
            else if (HeightType == DynamicContentHeightType.Pixels)
            {
                return String.Format("height:{0}px !important;", Height);
            }

            return String.Empty;
        }

        #endregion Private Methods
    }
}
