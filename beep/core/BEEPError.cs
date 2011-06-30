/*
* BEEPError.java  $Revision: 1.1 $ $Date: 2004/09/21 08:46:56 $
*
* Copyright (c) 2001 Invisible Worlds, Inc.  All rights reserved.
* Copyright (c) 2002 Huston Franklin.  All rights reserved.
*
* The contents of this file are subject to the Blocks Public License (the
* "License"); You may not use this file except in compliance with the License.
*
* You may obtain a copy of the License at http://www.beepcore.org/
*
* Software distributed under the License is distributed on an "AS IS" basis,
* WITHOUT WARRANTY OF ANY KIND, either express or implied.  See the License
* for the specific language governing rights and limitations under the
* License.
*
*/
using System;
using System.Xml;

namespace beepcore.beep.core
{
	/// <summary>Class BEEPError</summary>
	/// <author>Eric Dixon</author>
	/// <author>Huston Franklin</author>
	/// <author>Jay Kint</author>
	/// <author>Scott Pead</author>
	/// <version>$Revision: 1.1 $, $Date: 2004/09/21 08:46:56 $</version>
	public class BEEPError:BEEPException
	{
		/// <summary>Method getCode</summary>
		/// <returns>the BEEP ERR code</returns>
		virtual public BEEPStatusCode Code
		{
			get
			{
				return this.code;
			}
			
		}
		/// <summary>Method getXMLLang</summary>
		/// <returns>the BEEP ERR xmllang</returns>
		virtual public string XMLLang
		{
			get
			{
				return this.xmlLang;
			}
			
		}
		/// <summary>Method getDiagnostic</summary>
		/// <returns>the BEEP ERR diagnostic message</returns>
		virtual public string Diagnostic
		{
			get
			{
				return this.Message;
			}
			
		}
		
		private const string ERR_MALFORMED_XML_MSG = "Malformed XML";
		
		private BEEPStatusCode code;
		private string xmlLang = null;
		
		/// <summary>Constructor BEEPError</summary>
		/// <param name="code"></param>
		/// <param name="diagnostic"></param>
		/// <param name="xmlLang"></param>
		public BEEPError(BEEPStatusCode code, string diagnostic, string xmlLang):base(diagnostic)
		{
			this.xmlLang = xmlLang;
			this.code = code;
		}
		
		/// <summary>Constructor BEEPError</summary>
		/// <param name="code"></param>
		/// <param name="diagnostic"></param>
		public BEEPError(BEEPStatusCode code, string diagnostic):this(code,diagnostic,null)
		{
		}
		
		/// <summary>Constructor BEEPError</summary>
		/// <param name="code"></param>
		public BEEPError(BEEPStatusCode code):this(code,"")
		{
		}
		
		/// <summary>Method createErrorMessage</summary>
		/// <returns>the XML error element that can be sent in a BEEP ERR message</returns>
		public virtual string createErrorMessage()
		{
			return createErrorMessage(this.code, this.Message, this.xmlLang);
		}
		
		/// <summary>Creates a <code>String</code> for an error element that can be sent
		/// in a BEEP ERR message.</summary>
		/// <param name="code">Error code.</param>
		/// <param name="diagnostic">Error diagnostic.</param>
		public static string createErrorMessage(BEEPStatusCode code, string diagnostic)
		{
			return createErrorMessage(code, diagnostic, null);
		}
		
		/// <summary>Creates a <code>String</code> for an error element that can be sent
		/// in a BEEP ERR message.</summary>
		/// <param name="code">Error code.</param>
		/// <param name="diagnostic">Error diagnostic.</param>
		/// <param name="xmlLang">Language of the diagnostic message.</param>
		public static string createErrorMessage(BEEPStatusCode code, string diagnostic, string xmlLang)
		{
			System.Text.StringBuilder sb = new System.Text.StringBuilder(128);
			
			sb.Append("<error code='");
			sb.Append(code);
			
			if ((object) xmlLang != null)
			{
				sb.Append("' xml:lang='");
				sb.Append(xmlLang);
			}
			
			if ((object) diagnostic != null)
			{
				sb.Append("' >");
				sb.Append(diagnostic);
				sb.Append("</error>");
			}
			else
			{
				sb.Append("' />");
			}
			
			return sb.ToString();
		}
		
		/// <summary>Method convertMessageERRToException</summary>
		/// <param name="message"></param>
		/// <returns>New <code>BEEPError</code> for the specified BEEP ERR message</returns>
		/// <exception cref="BEEPException" />
		protected internal static BEEPError convertMessageERRToException(IMessage message)
		{
			if (message.MessageType != MessageType.MESSAGE_TYPE_ERR)
			{
				throw new System.ArgumentException("messageType != ERR");
			}
			
			// parse the stream
			XmlDocument doc = new XmlDocument();
			
			try
			{
				doc.Load(message.DataStream.InputStream);
			}
			catch (System.Xml.XmlException e)
			{
				throw new BEEPException(ERR_MALFORMED_XML_MSG, e);
			}
			
			XmlElement topElement = doc.DocumentElement;
			
			if (topElement == null)
			{
				throw new BEEPException(ERR_MALFORMED_XML_MSG);
			}
			
			// check for <error>
			string elementName = topElement.Name;
			
			if ((object) elementName == null)
			{
				throw new BEEPException(ERR_MALFORMED_XML_MSG);
			}
			else if (!elementName.Equals("error"))
			{
				throw new BEEPException("Unknown operation element");
			}
			
			string code = topElement.GetAttribute("code");
			
			if ((object) code == null)
			{
				throw new BEEPException(ERR_MALFORMED_XML_MSG);
			}
			
			// this attribute is implied
			string xmlLang = topElement.GetAttribute("xml:lang");
			System.Xml.XmlNode dataNode = topElement.FirstChild;
			string data = null;
			
			if (dataNode != null)
			{
				data = dataNode.InnerText;
			}
			
			return new BEEPError((BEEPStatusCode)System.Int32.Parse(code), data, xmlLang);
		}
		
	}

	public enum BEEPStatusCode
	{
		/// <summary>Success </summary>
		SUCCESS = 200,
		
		/// <summary>Service not available </summary>
		SERVICE_NOT_AVAILABLE = 421,
			
		/// <summary>Requested action not taken (e.g., lock already in use) </summary>
		REQUESTED_ACTION_NOT_TAKEN = 450,
			
		/// <summary>Requested action aborted (e.g., local error in processing) </summary>
		REQUESTED_ACTION_ABORTED = 451,
			
		/// <summary>Temporary authentication failure </summary>
		TEMPORARY_AUTHENTICATION_FAILURE = 454,
			
		/// <summary>General syntax error (e.g., poorly-formed XML) </summary>
		GENERAL_SYNTAX_ERROR = 500,
			
		/// <summary>Syntax error in parameters (e.g., non-valid XML) </summary>
		PARAMETER_ERROR = 501,
			
		/// <summary>Parameter not implemented </summary>
		PARAMETER_NOT_IMPLEMENTED = 504,
			
		/// <summary>Authentication required </summary>
		AUTHENTICATION_REQUIRED = 530,
			
		/// <summary>Authentication mechanism insufficient (e.g., too weak,
		/// sequence exhausted, etc.)</summary>
		AUTHENTICATION_INSUFFICIENT = 534,
			
		/// <summary>Authentication failure </summary>
		AUTHENTICATION_FAILURE = 535,
			
		/// <summary>Action not authorized for user </summary>
		ACTION_NOT_AUTHORIZED = 537,
			
		/// <summary>Authentication mechanism requires encryption </summary>
		AUTHENTICATION_REQUIRES_ENCRYPTION = 538,
			
		/// <summary>requested action not taken (e.g., no requested profiles are
		/// acceptable)</summary>
		REQUESTED_ACTION_NOT_TAKEN2 = 550,
			
		/// <summary>Parameter invalid </summary>
		PARAMETER_INVALID = 553,
			
		/// <summary>Transaction failed (e.g. policy violation). </summary>
		TRANSACTION_FAILED = 554
	}
}