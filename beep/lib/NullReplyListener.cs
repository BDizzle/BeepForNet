/*
* EchoProfile.java    $Revision: 1.1 $ $Date: 2004/09/21 08:46:57 $
*
* Copyright (c) 2001 Invisible Worlds, Inc.  All rights reserved.
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
using beepcore.beep.core;
namespace beepcore.beep.lib
{
	
	/// <summary>This class acts as a sink for all replies to the <code>sendMSG()</code>.
	/// The reply is received by <code>NullReplyListener</code>, freed from the
	/// channel's receive buffers and discarded.</summary>
	/// <author>Eric Dixon</author>
	/// <author>Huston Franklin</author>
	/// <author>Jay Kint</author>
	/// <author>Scott Pead</author>
	/// <version>$Revision: 1.1 $, $Date: 2004/09/21 08:46:57 $</version>
	public class NullReplyListener : ReplyListener
	{
		public static NullReplyListener Listener
		{
			get
			{
				return listener;
			}
			
		}

		private static NullReplyListener listener = new NullReplyListener();
		
		private NullReplyListener()
		{
		}
		
		public virtual void receiveRPY(IMessage message)
		{
			message.DataStream.close();
		}

		public virtual void receiveERR(IMessage message)
		{
			message.DataStream.close();
		}
		
		public virtual void receiveANS(IMessage message)
		{
			message.DataStream.close();
		}
		
		public virtual void receiveNUL(IMessage message)
		{
		}
	}
}