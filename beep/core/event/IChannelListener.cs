/*
* ChannelListener.java  $Revision: 1.1 $ $Date: 2004/09/21 08:46:57 $
*
* Copyright (c) 2001 Huston Franklin.  All rights reserved.
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
namespace beepcore.beep.core.@event
{
	/// <summary>This is an interface defining the methods that must be implemented
	/// to listen for Channel Events.</summary>
	/// <author>Huston Franklin</author>
	/// <version>$Revision: 1.1 $, $Date: 2004/09/21 08:46:57 $</version>
	public interface IChannelListener
	{
		/// <summary>Invoked when the greeting has been received for a session.</summary>
		void  channelStarted(ChannelEventArgs e);
		
		/// <summary>Invoked when the greeting has been received for a session.</summary>
		void  channelClosed(ChannelEventArgs e);
	}
}