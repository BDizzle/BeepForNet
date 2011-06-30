/*
* MessageMSG.java  $Revision: 1.1 $ $Date: 2004/09/21 08:46:56 $
*
* Copyright (c) 2003 Huston Franklin.  All rights reserved.
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
namespace beepcore.beep.core
{
	
	/// <summary>This interface represents the operations available for messages of type MSG.</summary>
	/// <author>Huston Franklin</author>
	/// <version>$Revision: 1.1 $, $Date: 2004/09/21 08:46:56 $</version>
	public interface IMessageMSG : IMessage
	{
		/// <summary>Sends an ANS reply to this MSG message.</summary>
		/// <param name="stream">Payload to be sent.</param>
		/// <seealso cref="OutputDataStream"></seealso>
		/// <seealso cref="MessageStatus"></seealso>
		/// <seealso cref="#sendNUL"></seealso>
		MessageStatus sendANS(OutputDataStream stream);
		
		/// <summary>Sends an ERR reply to this MSG message.</summary>
		/// <param name="error">Error to send in the form of <code>BEEPError</code>.</param>
		/// <seealso cref="BEEPError"></seealso>
		/// <seealso cref="MessageStatus"></seealso>
		MessageStatus sendERR(BEEPError error);
		
		/// <summary>Sends an ERR reply to this MSG message.</summary>
		/// <param name="code"><code>code</code> attibute in <code>error</code> element.</param>
		/// <param name="diagnostic">Message for <code>error</code> element.</param>
		/// <seealso cref="MessageStatus"></seealso>
		MessageStatus sendERR(BEEPStatusCode code, string diagnostic);
		
		/// <summary>Sends an ERR reply to this MSG message.</summary>
		/// <param name="code"><code>code</code> attibute in <code>error</code> element.</param>
		/// <param name="diagnostic">Message for <code>error</code> element.</param>
		/// <param name="xmlLang"><code>xml:lang</code> attibute in <code>error</code>
		/// element.</param>
		/// <seealso cref="MessageStatus"></seealso>
		/// <returns>MessageStatus</returns>
		MessageStatus sendERR(BEEPStatusCode code, string diagnostic, string xmlLang);
		
		/// <summary>Sends a reply of type NUL to this MSG message. This is sent as the
		/// completion to a MSG/ANS/NUL message exchange.</summary>
		/// <seealso cref="MessageStatus"></seealso>
		/// <seealso cref="#sendANS"></seealso>
		/// <returns>MessageStatus</returns>
		MessageStatus sendNUL();
		
		/// <summary>Sends a RPY reply to this MSG message.</summary>
		/// <param name="stream">Payload to be sent.</param>
		/// <seealso cref="OutputDataStream"></seealso>
		/// <seealso cref="MessageStatus"></seealso>
		/// <returns>MessageStatus</returns>
		MessageStatus sendRPY(OutputDataStream stream);
	}
}