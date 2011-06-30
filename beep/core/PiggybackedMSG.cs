/*
* RequestHandler.java  $Revision: 1.1 $ $Date: 2004/09/21 08:46:56 $
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
using BufferSegment = beepcore.beep.util.BufferSegment;
using System.Text;
namespace beepcore.beep.core
{
	
	/// <summary>This class is used to wrap piggybacked data from the start channel request.</summary>
	/// <author>Huston Franklin</author>
	/// <version>$Revision: 1.1 $, $Date: 2004/09/21 08:46:56 $</version>
	class PiggybackedMSG:MessageMSGImpl //, MessageMSG
	{
		
		internal PiggybackedMSG(ChannelImpl channel, byte[] data, bool base64encoding):base(channel, -1, new InputDataStream())
		{
			this.DataStream.add(new BufferSegment(Encoding.ASCII.GetBytes("\r\n")));
			this.DataStream.add(new BufferSegment(data));
			this.DataStream.setComplete();
		}
		
		public override MessageStatus sendANS(OutputDataStream stream)
		{
			throw new BEEPException("ANS reply not valid for piggybacked requests");
		}
		
		public override MessageStatus sendERR(BEEPError error)
		{
			throw new BEEPException("ERR reply not valid for piggybacked requests");
		}
		
		public override MessageStatus sendERR(BEEPStatusCode code, string diagnostic)
		{
			throw new BEEPException("ERR reply not valid for piggybacked requests");
		}
		
		public override MessageStatus sendERR(BEEPStatusCode code, string diagnostic, string xmlLang)
		{
			throw new BEEPException("ERR reply not valid for piggybacked requests");
		}
		
		public override MessageStatus sendNUL()
		{
			throw new BEEPException("ANS reply not valid for piggybacked requests");
		}
		
		public override MessageStatus sendRPY(OutputDataStream stream)
		{
			SessionImpl s = (SessionImpl) this.channel.Session;
			System.IO.MemoryStream tmp = new System.IO.MemoryStream(SessionImpl.MAX_PCDATA_SIZE);
			string data;
			
			while (stream.availableSegment())
			{
				BufferSegment b = stream.getNextSegment(SessionImpl.MAX_PCDATA_SIZE);
				
				tmp.Write(b.Data, 0, b.Length);
			}
			
			data = System.Text.Encoding.UTF8.GetString(tmp.ToArray());
			int crlf = data.IndexOf("\r\n");
			if (crlf != - 1)
			{
				data = data.Substring(crlf + "\r\n".Length);
			}
			
			try
			{
				s.sendProfile(this.channel.getProfile(), data, this.channel);
			}
			catch (BEEPException e)
			{
				s.terminate("Error sending profile. " + e.Message);
				throw e;
			}
			
			if (this.channel.getState() != core.ChannelState.STATE_TUNING)
			{
				this.channel.setState(core.ChannelState.STATE_ACTIVE);
				((SessionImpl) this.channel.Session).enableIO();
			}
			
			s.fireChannelStarted(this.channel);
			
			return new MessageStatus(this.channel, core.MessageType.MESSAGE_TYPE_RPY, - 1, stream);
		}
	}
}