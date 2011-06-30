/*
* Blob.java            $Revision: 1.1 $ $Date: 2004/09/21 08:46:58 $
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
using Log = org.apache.commons.logging.Log;
using LogFactory = org.apache.commons.logging.LogFactory;
using Element = System.Xml.XmlElement;
using Node = System.Xml.XmlNode;
using Document = System.Xml.XmlDocument;
namespace org.beepcore.beep.profile.sasl
{
	
	
	/// <summary> The Blob class tries to abstract some of the complexity inherent in
	/// dealing with the XML <blob> elements that are used in the SASL 
	/// profiles.  This class may eventually belong in CORE, but until then.
	/// One other developmental issue - it might be simpler to just use 
	/// strings all the time.  I just try to avoid string comparison, but
	/// I'm not sure this effort is buying anything.  I need to examine
	/// how the status is used...and may well go back to String representation
	/// of status alone.
	/// @todo move Blob to core
	/// 
	/// </summary>
	/// <author>  Eric Dixon
	/// </author>
	/// <author>  Huston Franklin
	/// </author>
	/// <author>  Jay Kint
	/// </author>
	/// <author>  Scott Pead
	/// </author>
	/// <version>  $Revision: 1.1 $, $Date: 2004/09/21 08:46:58 $
	/// 
	/// </version>
	public class Blob
	{
		private void  InitBlock()
		{
			log = LogFactory.getLog(this.GetType());
		}
		/// <summary> Returns the status used in the blob - can be 'none'.</summary>
		virtual public System.String Status
		{
			get
			{
				return statusMappings[status];
			}
			
		}
		/// <summary> Returns the blob data as a <code>String</code>.</summary>
		virtual public System.String Data
		{
			get
			{
				if (decodedData == null)
				{
					return "";
				}
				return new System.String(SupportClass.ToCharArray(SupportClass.ToByteArray(decodedData)));
			}
			
		}
		/// <summary> Returns the blob data as bytes.</summary>
		virtual public byte[] DataBytes
		{
			get
			{
				return decodedData;
			}
			
		}
		// Constants
		// Status Constants
		//UPGRADE_NOTE: Final was removed from the declaration of 'ABORT '. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1003"'
		public static readonly System.String ABORT = SASLProfile.SASL_STATUS_ABORT;
		//UPGRADE_NOTE: Final was removed from the declaration of 'COMPLETE '. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1003"'
		public static readonly System.String COMPLETE = SASLProfile.SASL_STATUS_COMPLETE;
		//UPGRADE_NOTE: Final was removed from the declaration of 'CONTINUE '. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1003"'
		public static readonly System.String CONTINUE = SASLProfile.SASL_STATUS_CONTINUE;
		//UPGRADE_NOTE: Final was removed from the declaration of 'NONE '. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1003"'
		public static readonly System.String NONE = SASLProfile.SASL_STATUS_NONE;
		public const int STATUS_NONE = 0;
		public const int STATUS_ABORT = 1;
		public const int STATUS_CONTINUE = 2;
		public const int STATUS_COMPLETE = 3;
		public const int STATUS_LIMIT = 4;
		// Data Constants
		public const int DEFAULT_BLOB_SIZE = 1024;
		// Error Constants
		private const System.String ERR_INVALID_STATUS_VALUE = "Invalid SASL Status for Blob";
		private const System.String ERR_XML_PARSE_FAILURE = "Failed to parse xml";
		// XML Fragment Constants
		
		
		// Class data
		private static System.String[] statusMappings;
		//UPGRADE_TODO: Class 'sun.misc.BASE64Decoder' was not converted. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1095"'
		private static BASE64Decoder decoder;
		//UPGRADE_TODO: Class 'sun.misc.BASE64Encoder' was not converted. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1095"'
		private static BASE64Encoder encoder;
		private static bool initialized = false;
		
		// Data
		//UPGRADE_NOTE: The initialization of  'log' was moved to method 'InitBlock'. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1005"'
		private Log log;
		
		private int status;
		private System.String blobData;
		private System.String stringified;
		private byte[] decodedData;
		
		/// <summary> This is the Constructor for those that want to create and send a blob.
		/// 
		/// </summary>
		/// <param name="status">the status to construct the blob with (see the constants
		/// in this class).
		/// 
		/// @throws SASLException
		/// </param>
		public Blob(int status)
		{
			InitBlock();
			if (log.isDebugEnabled())
			{
				log.debug("Created blob=>" + status);
			}
			if (!initialized)
			{
				init();
			}
			
			// Validate status
			if (!validateStatus(status))
			{
				throw new SASLException(ERR_INVALID_STATUS_VALUE);
			}
			
			this.status = status;
			
			System.Text.StringBuilder buff = new System.Text.StringBuilder(DEFAULT_BLOB_SIZE);
			buff.Append("<blob ");
			
			if (status != STATUS_NONE)
			{
				buff.Append("status='");
				buff.Append(statusMappings[status]);
				buff.Append('\'');
			}
			
			if ((System.Object) blobData == null)
			{
				buff.Append("/>");
			}
			else
			{
				buff.Append(">");
				buff.Append(blobData);
				buff.Append("</blob>");
			}
			
			stringified = buff.ToString();
			if (log.isDebugEnabled())
			{
				log.debug("Created blob=>" + stringified);
			}
		}
		
		/// <summary> This is the Constructor for those that want to create and send a blob.
		/// 
		/// </summary>
		/// <param name="status">the status to construct the blob with (see the constants
		/// in this class).
		/// </param>
		/// <param name="data">the data to be embedded in the blob element
		/// 
		/// @throws SASLException
		/// </param>
		public Blob(int status, System.String data)
		{
			InitBlock();
			if (log.isDebugEnabled())
			{
				log.debug("Created blob=>" + status + "," + data);
			}
			if (!initialized)
			{
				init();
			}
			
			// Validate status
			if (!validateStatus(status))
			{
				throw new SASLException(ERR_INVALID_STATUS_VALUE);
			}
			
			this.status = status;
			if ((System.Object) data != null)
			{
				try
				{
					//UPGRADE_ISSUE: Method 'java.lang.String.getBytes' was not converted. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1000_javalangStringgetBytes_javalangString"'
					this.decodedData = System.Text.Encoding.UTF8.GetBytes(data);
				}
				catch (System.IO.IOException e)
				{
					throw new System.SystemException("UTF-8 encoding not supported");
				}
				
				//UPGRADE_TODO: Method 'sun.misc.CharacterEncoder.encodeBuffer' was not converted. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1095"'
				this.blobData = encoder.encodeBuffer(this.decodedData);
			}
			
			System.Text.StringBuilder buff = new System.Text.StringBuilder(DEFAULT_BLOB_SIZE);
			buff.Append("<blob ");
			
			if (status != STATUS_NONE)
			{
				buff.Append("status='");
				buff.Append(statusMappings[status]);
				buff.Append('\'');
			}
			
			if ((System.Object) blobData == null)
			{
				buff.Append("/>");
			}
			else
			{
				buff.Append(">");
				buff.Append(blobData);
				buff.Append("</blob>");
			}
			
			stringified = buff.ToString();
			if (log.isDebugEnabled())
			{
				log.debug("Created blob=>" + stringified);
			}
		}
		
		/// <summary> This is the Constructor for those that want to create and send a blob.
		/// 
		/// </summary>
		/// <param name="status">the status to construct the blob with (see the constants
		/// in this class).
		/// </param>
		/// <param name="data">the data to be embedded in the blob element
		/// 
		/// @throws SASLException
		/// </param>
		public Blob(int status, byte[] data)
		{
			InitBlock();
			if (!initialized)
			{
				init();
			}
			
			// Validate status
			if (!validateStatus(status))
			{
				throw new SASLException(ERR_INVALID_STATUS_VALUE);
			}
			
			this.status = status;
			if (data != null)
			{
				this.decodedData = data;
				//UPGRADE_TODO: Method 'sun.misc.CharacterEncoder.encodeBuffer' was not converted. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1095"'
				this.blobData = encoder.encodeBuffer(data);
			}
			
			System.Text.StringBuilder buff = new System.Text.StringBuilder(DEFAULT_BLOB_SIZE);
			buff.Append("<blob ");
			
			if (status != STATUS_NONE)
			{
				buff.Append("status='");
				buff.Append(statusMappings[status]);
				buff.Append('\'');
			}
			
			if ((System.Object) blobData == null)
			{
				buff.Append("/>");
			}
			else
			{
				buff.Append(">");
				buff.Append(blobData);
				buff.Append("</blob>");
			}
			
			stringified = buff.ToString();
			if (log.isDebugEnabled())
			{
				log.debug("Created blob=>" + stringified);
			}
		}
		
		/// <summary> Constructor for those that want to 'receive' or 'digest' a blob.
		/// 
		/// </summary>
		/// <param name="blob">The data to digest.
		/// 
		/// @throws SASLException in the event that errors occur during the
		/// parsing of the blob passed in.
		/// </param>
		public Blob(System.String blob)
		{
			InitBlock();
			if (log.isDebugEnabled())
			{
				log.debug("Receiving blob of=>" + blob);
			}
			if (!initialized)
			{
				init();
			}
			
			stringified = blob;
			// Parse out the status if there is any
			System.String statusString = extractStatusFromBlob(blob);
			this.status = STATUS_NONE;
			if ((System.Object) statusString != null)
			{
				/// @TODO && statusString.equals("") == false
				for (int i = 0; i < STATUS_LIMIT; i++)
				{
					if (statusMappings[i].Equals(statusString))
					{
						status = i;
						break;
					}
				}
				statusString = statusMappings[status];
			}
			
			// Parse out the data if there is any
			blobData = extractDataFromBlob(blob);
			if ((System.Object) blobData != null)
			{
				try
				{
					//UPGRADE_TODO: Method 'sun.misc.CharacterDecoder.decodeBuffer' was not converted. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1095"'
					decodedData = decoder.decodeBuffer(blobData);
				}
				catch (System.IO.IOException x)
				{
					//UPGRADE_TODO: The equivalent in .NET for method 'java.lang.Throwable.getMessage' may return a different value. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1043"'
					throw new SASLException(x.Message);
				}
			}
			else
			{
				decodedData = null;
			}
			
			if (status == STATUS_NONE && (System.Object) blobData == null)
			{
				throw new SASLException("No valid data in blob");
			}
			
			if (log.isDebugEnabled())
			{
				log.debug("Received Blob of =>" + stringified);
			}
		}
		
		/// <summary> Method init is a static method that prepares some of the
		/// XML parsing and Base64Encoding/Decoding machinery needed
		/// to understand blobs.  This functionality pops up elsewhere
		/// in the library and should probably be moved ot some XML
		/// utility class.  It isn't there yet though.
		/// 
		/// </summary>
		private static void  init()
		{
			if (decoder == null)
			{
				//UPGRADE_TODO: Constructor 'sun.misc.BASE64Decoder.BASE64Decoder' was not converted. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1095"'
				decoder = new BASE64Decoder();
			}
			
			if (encoder == null)
			{
				//UPGRADE_TODO: Constructor 'sun.misc.BASE64Encoder.BASE64Encoder' was not converted. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1095"'
				encoder = new BASE64Encoder();
			}
			
			if (statusMappings == null)
			{
				statusMappings = new System.String[STATUS_LIMIT];
				statusMappings[STATUS_NONE] = NONE;
				statusMappings[STATUS_ABORT] = ABORT;
				statusMappings[STATUS_CONTINUE] = CONTINUE;
				statusMappings[STATUS_COMPLETE] = COMPLETE;
			}
			initialized = true;
		}
		
		/// <summary> Returns a representation of the Blob as it would be sent
		/// out on the wire (with the data encoded).
		/// 
		/// </summary>
		public override System.String ToString()
		{
			return stringified;
		}
		
		/// <summary> Routines to parse XML blobs and extract either status
		/// attributes or data (which is sometimes clear, sometimes Base64,
		/// and potentially sometimes in other formats).  This routine
		/// doesn't decode anything
		/// 
		/// @todo move this to some util class
		/// 
		/// </summary>
		/// <param name="blob">the data to be processed
		/// 
		/// </param>
		/// <returns> the top <code>Element</code>
		/// 
		/// @throws SASLException
		/// </returns>
		//UPGRADE_TODO: Interface 'org.w3c.dom.Element' was not converted. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1095"'
		private Element processMessage(System.String blob)
		{
			
			// parse the stream
			//UPGRADE_TODO: Interface 'org.w3c.dom.Document' was not converted. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1095"'
			Document doc = new Document();
			
			try
			{
				log.debug("Tuning Profile Parse Routine");
				
				//UPGRADE_TODO: Method 'javax.xml.parsers.DocumentBuilder.parse' was not converted. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1095"'
				doc.Load(new System.IO.MemoryStream(SupportClass.ToByteArray(blob)));
				
				log.debug("parsed message");
			}
			catch (System.Exception e)
			{
				log.debug("Error parsing", e);
				throw new SASLException(ERR_XML_PARSE_FAILURE);
			}
			
			//UPGRADE_TODO: Interface 'org.w3c.dom.Element' was not converted. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1095"'
			//UPGRADE_TODO: Method 'org.w3c.dom.Document.getDocumentElement' was not converted. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1095"'
			Element topElement = doc.DocumentElement;
			
			if (topElement == null)
			{
				throw new SASLException(ERR_XML_PARSE_FAILURE);
			}
			
			return topElement;
		}
		
		/// <summary> Method extractStatusFromBlob
		/// 
		/// </summary>
		/// <param name="blob">is the data to extract from
		/// </param>
		/// <returns> String the value of the status attribute
		/// 
		/// @throws SASLException if a parsing error occurs
		/// or the input is invalid in some other way.
		/// 
		/// </returns>
		private System.String extractStatusFromBlob(System.String blob)
		{
			if (((System.Object) blob == null) || (blob.IndexOf("<blob ") == - 1))
			{
				return null;
			}
			
			//UPGRADE_TODO: Interface 'org.w3c.dom.Element' was not converted. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1095"'
			Element top = processMessage(blob);
			
			//UPGRADE_TODO: Method 'org.w3c.dom.Element.getTagName' was not converted. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1095"'
			if (!top.Name.Equals("blob"))
			{
				throw new SASLException(ERR_XML_PARSE_FAILURE);
			}
			
			//UPGRADE_TODO: Method 'org.w3c.dom.Element.GetAttribute' was not converted. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1095"'
			return top.GetAttribute("status");
		}
		
		/// <summary> Method extractStatusFromBlob
		/// 
		/// </summary>
		/// <param name="blob">is the data to extract from
		/// </param>
		/// <returns> String the value of the blob element
		/// 
		/// @throws SASLException if a parsing error occurs
		/// or the input is invalid in some other way.
		/// 
		/// </returns>
		private System.String extractDataFromBlob(System.String blob)
		{
			if ((System.Object) blob == null)
			{
				return null;
			}
			
			System.String result;
			//UPGRADE_TODO: Interface 'org.w3c.dom.Element' was not converted. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1095"'
			Element top = processMessage(blob);
			
			//UPGRADE_TODO: Method 'org.w3c.dom.Element.getTagName' was not converted. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1095"'
			if (!top.Name.Equals("blob"))
			{
				throw new SASLException(ERR_XML_PARSE_FAILURE);
			}
			
			//UPGRADE_TODO: Interface 'org.w3c.dom.Node' was not converted. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1095"'
			//UPGRADE_TODO: Method 'org.w3c.dom.Node.getFirstChild' was not converted. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1095"'
			Node dataNode = top.FirstChild;
			
			if (dataNode == null)
			{
				return null;
			}
			
			//UPGRADE_TODO: Method 'org.w3c.dom.Node.getNodeValue' was not converted. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1095"'
			//UPGRADE_TODO: Method 'org.w3c.dom.Node.getFirstChild' was not converted. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1095"'
			return top.FirstChild.InnerText;
		}
		
		/// <summary> Base64 Help routines so we don't have to expose them
		/// to subclasses and others.
		/// 
		/// @todo move this to some util class
		/// 
		/// </summary>
		/// <param name="data">the data to encode
		/// </param>
		/// <returns> String the encoded data.
		/// 
		/// @throws SASLException
		/// </returns>
		private static System.String encodeData(System.String data)
		{
			try
			{
				//UPGRADE_TODO: Method 'sun.misc.CharacterEncoder.encodeBuffer' was not converted. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1095"'
				return new System.Text.StringBuilder(encoder.encodeBuffer((SupportClass.ToByteArray(data)))).ToString();
			}
			catch (System.Exception x)
			{
				throw new SASLException("Base64 encode or decode failure");
			}
		}
		
		/// <summary> Method decodeData simply checks values of the blob's
		/// status attribute against 'valid' values.
		/// 
		/// </summary>
		/// <param name="status">the string to be checked
		/// </param>
		/// <returns> boolean whether or not it's valid or not
		/// 
		/// @throws SASLException
		/// 
		/// </returns>
		private static bool validateStatus(System.String status)
		{
			if (!status.Equals(ABORT) && !status.Equals(NONE) && !status.Equals(CONTINUE) && !status.Equals(COMPLETE))
			{
				return false;
			}
			
			return true;
		}
		
		/// <summary> Method validateStatus validates a status value for an instance
		/// of the blob class.
		/// 
		/// </summary>
		/// <param name="">data
		/// 
		/// @throws SASLException
		/// 
		/// </param>
		private static bool validateStatus(int status)
		{
			if (status < STATUS_NONE || status > STATUS_COMPLETE)
			{
				return false;
			}
			
			return true;
		}
	}
}