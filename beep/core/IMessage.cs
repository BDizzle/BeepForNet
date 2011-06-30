/*
* Message.java            $Revision: 1.1 $ $Date: 2004/09/21 08:46:56 $
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
using System;
namespace beepcore.beep.core
{
	public enum MessageType {
		/// <summary>Uninitialized <code>Message</code>.</summary>
		MESSAGE_TYPE_UNK = 0,
		/// <summary>BEEP MSG message.</summary>
		MESSAGE_TYPE_MSG = 1,
		/// <summary>BEEP RPY message.</summary>
		MESSAGE_TYPE_RPY = 2,
		/// <summary>BEEP ERR message.</summary>
		MESSAGE_TYPE_ERR = 3,
		/// <summary>BEEP ANS message.</summary>
		MESSAGE_TYPE_ANS = 4,
		/// <summary>BEEP NUL message.</summary>
		MESSAGE_TYPE_NUL = 5
	}

	/// <summary>This interface represents the operations available for all types of
	/// messages.</summary>
	/// <author>Huston Franklin</author>
	/// <version>$Revision: 1.1 $, $Date: 2004/09/21 08:46:56 $</version>
	public interface IMessage
	{
		/// <summary>Returns <code>InputDataStream</code> containing the payload for this
		/// <code>Message</code>.</summary>
		/// <seealso cref="InputDataStream"></seealso>
		InputDataStream DataStream
		{
			get;			
		}

		/// <summary>Returns the <code>Channel</code> on which this <code>Message</code>
		/// was received.</summary>
		/// <seealso cref="Channel"></seealso>
		IChannel Channel
		{
			get;
		}

		/// <summary>Returns the message number of this <code>Message</code>.</summary>
		int Msgno
		{
			get;
		}

		/// <summary>Returns the answer number of this <code>Message</code>.</summary>
		int Ansno
		{
			get;
		}

		/// <summary>Returns the message type of this <code>Message</code>.</summary>
		core.MessageType MessageType
		{
			get;
		}
		
#if MESSAGELISTENER
		/// <deprecated>use method on MessageMSG instead.</deprecated>
		MessageStatus sendANS(OutputDataStream stream);
		
		/// <deprecated>use method on MessageMSG instead.</deprecated>
		MessageStatus sendERR(BEEPError error);
		
		/// <deprecated>use method on MessageMSG instead.</deprecated>
		MessageStatus sendERR(int code, string diagnostic);
		
		/// <deprecated>use method on MessageMSG instead.</deprecated>
		MessageStatus sendERR(int code, string diagnostic, string xmlLang);
		
		/// <deprecated>use method on MessageMSG instead.</deprecated>
		MessageStatus sendNUL();
		
		/// <deprecated>use method on MessageMSG instead.</deprecated>
		MessageStatus sendRPY(OutputDataStream stream);
#endif
	}
}