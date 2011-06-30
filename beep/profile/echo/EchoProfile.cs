/*
* EchoProfile.java    $Revision: 1.1 $ $Date: 2004/09/21 08:46:58 $
*
* Copyright (c) 2001 Invisible Worlds, Inc.  All rights reserved.
* Copyright (c) 2002,2003 Huston Franklin.  All rights reserved.
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
using Log = beepcore.logging.Log;
using LogFactory = beepcore.logging.LogFactory;
using beepcore.beep.core;
using beepcore.beep.profile;
using BufferSegment = beepcore.beep.util.BufferSegment;
namespace beepcore.beep.profile.echo
{
	/// <summary>This is the Echo profile implementation</summary>
	/// <author>Eric Dixon</author>
	/// <author>Huston Franklin</author>
	/// <author>Jay Kint</author>
	/// <author>Scott Pead</author>
	/// <version>$Revision: 1.1 $, $Date: 2004/09/21 08:46:58 $</version>
	public class EchoProfile : IProfile, IStartChannelListener, IRequestHandler
	{
		public const string ECHO_URI = "http://xml.resource.org/profiles/NULL/ECHO";
		
		private Log log;
		
		public EchoProfile()
		{
			log = LogFactory.getLog(this.GetType());
		}
		
		public virtual IStartChannelListener Init(string uri, ProfileConfiguration config)
		{
			return this;
		}
		
		public virtual void StartChannel(IChannel channel, string encoding, string data)
		{
			log.debug("EchoCCL StartChannel Callback");
			channel.setRequestHandler(this);
		}
		
		public virtual void CloseChannel(IChannel channel)
		{
			log.debug("EchoCCL CloseChannel Callback");
			channel.setRequestHandler(null);
		}
		
		public virtual bool AdvertiseProfile(ISession session)
		{
			return true;
		}
		
		public virtual void ReceiveMSG(IMessageMSG message)
		{
			OutputDataStream data = new OutputDataStream();
			InputDataStream ds = message.DataStream;
			
			while (true)
			{
				try
				{
					BufferSegment b = ds.waitForNextSegment();
					if (b == null)
					{
						break;
					}
					data.add(b);
				}
				catch (System.Threading.ThreadInterruptedException e)
				{
					message.Channel.Session.terminate(e.Message);
					return ;
				}
			}
			
			data.setComplete();
			
			try
			{
				message.sendRPY(data);
			}
			catch (BEEPException)
			{
				try
				{
					message.sendERR(BEEPStatusCode.REQUESTED_ACTION_ABORTED, "Error sending RPY");
				}
				catch (BEEPException x)
				{
					message.Channel.Session.terminate(x.Message);
				}
				return ;
			}
		}
	}
}