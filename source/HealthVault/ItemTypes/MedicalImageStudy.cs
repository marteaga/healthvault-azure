﻿// Copyright(c) Microsoft Corporation.
// This content is subject to the Microsoft Reference Source License,
// see http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx.
// All other rights reserved.

using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.ObjectModel;
using System.Xml.XPath;
using System.Xml;

namespace Microsoft.Health.ItemTypes
{
    /// <summary>
    /// Represents a health record item type that encapsulates a study containing medical images.
    /// </summary>
    /// 
    /// <remarks>
    /// DICOM data is stored in the named BLOB portion of the data type, and it is recommended 
    /// that normal DICOM conventions are used for naming.
    /// 
    /// An application may store XDS-i manifest information in XML format in the xds-i element.
    /// </remarks>
    /// 
    public class MedicalImageStudy : HealthRecordItem
    {
        /// <summary>
        /// Creates a new instance of the <see cref="MedicalImageStudy"/> class with default values.
        /// </summary>
        /// 
        /// <remarks>
        /// The item is not added to the health record until the
        /// <see cref="Microsoft.Health.HealthRecordAccessor.NewItem(HealthRecordItem)"/> method 
        /// is called.
        /// </remarks>
        /// 
        public MedicalImageStudy()
            : base(TypeId)
        {
        }

        /// <summary>
        /// Creates a new instance of the <see cref="MedicalImageStudy"/> class
        /// specifying mandatory values.
        /// </summary>
        /// 
        /// <remarks>
        /// The item is not added to the health record until the
        /// <see cref="Microsoft.Health.HealthRecordAccessor.NewItem(HealthRecordItem)"/> method 
        /// is called.
        /// </remarks>
        /// 
        /// <param name="series">
        /// Medical image study series
        /// </param>
        /// <param name="when">
        /// Time that the study was created.
        /// </param>
        /// 
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="when"/> parameter is <b>null</b>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// If <paramref name="series"/> parameter is <b>null</b> or doesn't contain any series.
        /// </exception>
        /// 
        public MedicalImageStudy(
            HealthServiceDateTime when,
            IEnumerable<MedicalImageStudySeries> series)
            : base(TypeId)
        {
            Validator.ThrowIfArgumentNull(series, "series", "MedicalImageStudySeriesMandatory");

            When = when;
            foreach (MedicalImageStudySeries imageSeries in series)
            {
                Series.Add(imageSeries);
            }

            Validator.ThrowArgumentExceptionIf(
                Series.Count == 0,
                "series",
                "MedicalImageStudySeriesMandatory");
        }

        /// <summary>
        /// Retrieves the unique identifier for the item type.
        /// </summary>
        /// 
        /// <value>
        /// A GUID.
        /// </value>
        /// 
        public new static readonly Guid TypeId =
            new Guid("c75651c8-548e-449f-8942-9e6379b0b88a");

        /// <summary>
        /// Populates the data from the specified XML.
        /// </summary>
        /// 
        /// <param name="typeSpecificXml">
        /// The XML to get the medical image study data from.
        /// </param>
        /// 
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="typeSpecificXml"/> parameter is <b>null</b>.
        /// </exception>
        /// 
        /// <exception cref="InvalidOperationException">
        /// If the first node in <paramref name="typeSpecificXml"/> is not
        /// a medical image study node.
        /// </exception>
        /// 
        protected override void ParseXml(IXPathNavigable typeSpecificXml)
        {
            Validator.ThrowIfArgumentNull(typeSpecificXml, "typeSpecificXml", "ParseXmlNavNull");

            XPathNavigator itemNav =
                typeSpecificXml.CreateNavigator().SelectSingleNode("medical-image-study");

            Validator.ThrowInvalidIfNull(itemNav, "MedicalImageStudyUnexpectedNode");

            if (_when == null)
            {
                _when = new HealthServiceDateTime();
            }
            _when.ParseXml(itemNav.SelectSingleNode("when"));
            _patientName = XPathHelper.GetOptNavValue(itemNav, "patient-name");
            _description = XPathHelper.GetOptNavValue(itemNav, "description");

            _series.Clear();
            foreach (XPathNavigator imageStudySeriesNav in itemNav.Select("series"))
            {
                MedicalImageStudySeries imageStudySeries = new MedicalImageStudySeries();
                imageStudySeries.ParseXml(imageStudySeriesNav);
                _series.Add(imageStudySeries);
            }

            _reason = XPathHelper.GetOptNavValue<CodableValue>(itemNav, "reason");
            _previewBlobName = XPathHelper.GetOptNavValue(itemNav, "preview-blob-name");

            _keyImages.Clear();
            foreach (XPathNavigator imageNav in itemNav.Select("key-images"))
            {
                MedicalImageStudySeriesImage image = new MedicalImageStudySeriesImage();
                image.ParseXml(imageNav);
                _keyImages.Add(image);
            }

            _studyInstanceUID = XPathHelper.GetOptNavValue(itemNav, "study-instance-uid");
        }

        /// <summary>
        /// Writes the XML representation of the medical image study into
        /// the specified XML writer.
        /// </summary>
        /// 
        /// <param name="writer">
        /// The XML writer into which the medical image study series should be 
        /// written.
        /// </param>
        /// 
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="writer"/> parameter is <b>null</b>.
        /// </exception>
        /// 
        /// <exception cref="HealthRecordItemSerializationException">
        /// If <see cref="When"/> is <b>null</b>.
        /// If <see cref="Series"/> collection is <b>null</b> or empty. 
        /// </exception>
        /// 
        public override void WriteXml(XmlWriter writer)
        {
            Validator.ThrowIfWriterNull(writer);
            Validator.ThrowSerializationIfNull(_when, "WhenNullValue");
            Validator.ThrowSerializationIf(
                _series == null || _series.Count == 0,
                "MedicalImageStudySeriesMandatory");

            writer.WriteStartElement("medical-image-study");

            _when.WriteXml("when", writer);
            XmlWriterHelper.WriteOptString(writer, "patient-name", _patientName);
            XmlWriterHelper.WriteOptString(writer, "description", _description);

            foreach (MedicalImageStudySeries imageStudySeries in _series)
            {
                imageStudySeries.WriteXml("series", writer);
            }

            XmlWriterHelper.WriteOpt<CodableValue>(writer, "reason", _reason);
            XmlWriterHelper.WriteOptString(writer, "preview-blob-name", _previewBlobName);

            foreach (MedicalImageStudySeriesImage image in _keyImages)
            {
                image.WriteXml("key-images", writer);
            }

            XmlWriterHelper.WriteOptString(writer, "study-instance-uid", _studyInstanceUID);

            writer.WriteEndElement();
        }

        /// <summary>
        /// Gets or sets the time that the study was created.
        /// </summary>
        /// 
        /// <remarks>
        /// This value corresponds to DICOM tags (0008, 0020) and (0008, 0030).
        /// </remarks>
        /// 
        /// <exception cref="ArgumentNullException">
        /// The <paramref name="value"/> parameter is <b>null</b>.
        /// </exception>
        /// 
        public HealthServiceDateTime When
        {
            get { return _when; }
            set
            {
                Validator.ThrowIfArgumentNull(value, "When", "WhenNullValue");
                _when = value;
            }
        }
        private HealthServiceDateTime _when;

        /// <summary>
        /// Gets or sets the name of the patient as contained in the medical image.
        /// </summary>
        /// 
        /// <remarks>
        /// This value corresponds to DICOM tag (0010, 0010).
        /// </remarks>
        /// 
        /// <exception cref="ArgumentException">
        /// The <paramref name="value"/> contains only whitespace.
        /// </exception>
        /// 
        public string PatientName
        {
            get { return _patientName; }
            set
            {
                Validator.ThrowIfStringIsWhitespace(value, "PatientName");
                _patientName = value;
            }
        }
        private string _patientName;

        /// <summary>
        /// Gets or sets a description of the study.
        /// </summary>
        /// 
        /// <remarks>
        /// This value corresponds to DICOM tag (0008, 1030).
        /// </remarks>
        /// 
        /// <exception cref="ArgumentException">
        /// The <paramref name="value"/> contains only whitespace.
        /// </exception>
        /// 
        public string Description
        {
            get { return _description; }
            set
            {
                Validator.ThrowIfStringIsWhitespace(value, "Description");
                _description = value;
            }
        }
        private string _description;

        /// <summary>
        /// Gets a collection of series.
        /// </summary>
        /// 
        public Collection<MedicalImageStudySeries> Series
        {
            get { return _series; }
        }
        private Collection<MedicalImageStudySeries> _series = new Collection<MedicalImageStudySeries>();

        /// <summary>
        /// Gets or sets the reason for the study.
        /// </summary>
        /// 
        /// <remarks>
        /// This value corresponds to DICOM tag (0032, 1030).
        /// 
        /// If there is no information about the reason the value should be set 
        /// to <b>null</b>.
        /// </remarks>
        /// 
        public CodableValue Reason
        {
            get { return _reason; }
            set { _reason = value; }
        }
        private CodableValue _reason;

        /// <summary>
        /// Gets or sets the name of the BLOB holding a smaller version of the image 
        /// suitable for web viewing or email.
        /// </summary>
        /// 
        /// <remarks>
        /// If there is no preview BLOB name the value should be set to <b>null</b>.
        /// Previews should be stored using the jpeg or png format.
        /// It is recommended that the shorter dimension of the image be no less than 500 pixels in size.
        /// </remarks>
        /// 
        /// <exception cref="ArgumentException">
        /// If <paramref name="value"/> contains only whitespace.
        /// </exception> 
        /// 
        public string PreviewBlobName
        {
            get { return _previewBlobName; }
            set
            {
                Validator.ThrowIfStringIsWhitespace(value, "PreviewBlobName");
                _previewBlobName = value;
            }
        }
        private string _previewBlobName;

        /// <summary>
        /// Gets the important images in the study.
        /// </summary>
        /// 
        /// <remarks>
        /// If there is no information about the key images the collection should be empty.
        /// </remarks>
        /// 
        public Collection<MedicalImageStudySeriesImage> KeyImages
        {
            get { return _keyImages; }
        }
        private Collection<MedicalImageStudySeriesImage> _keyImages = new Collection<MedicalImageStudySeriesImage>();

        /// <summary>
        /// Gets or sets the study instance UID.
        /// </summary>
        /// 
        /// <remarks>
        /// This value corresponds to DICOM tag (0020,000D).
        /// If there is no study instance UID the value should be set to <b>null</b>.
        /// </remarks>
        /// 
        /// <exception cref="ArgumentException">
        /// If <paramref name="value"/> contains only whitespace.
        /// </exception> 
        /// 
        public string StudyInstanceUID
        {
            get { return _studyInstanceUID; }
            set
            {
                Validator.ThrowIfStringIsWhitespace(value, "StudyInstanceUID");
                _studyInstanceUID = value;
            }
        }
        private string _studyInstanceUID;

        /// <summary>
        /// Gets a string representation of the medical image study.
        /// </summary>
        /// 
        /// <returns>
        /// A string representation of the medical image study.
        /// </returns>
        /// 
        public override string ToString()
        {
            StringBuilder result = new StringBuilder(200);

            if (!string.IsNullOrEmpty(Description) && !string.IsNullOrEmpty(Description.Trim()))
            {
                result.Append(Description);

            }

            if (Reason != null)
            {
                if (result.Length > 0)
                {
                    result.Append(ResourceRetriever.GetResourceString("ListSeparator"));
                }
                result.Append(Reason.ToString());
            }

            return result.ToString();
        }
    }
}
