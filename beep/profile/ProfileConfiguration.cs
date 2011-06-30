/*
* ProfileConfiguration.java            $Revision: 1.1 $ $Date: 2004/09/21 08:46:57 $
*
* Copyright (c) 2001 Invisible Worlds, Inc.  All rights reserved.
* Copyright (c) 2002 Huston Franklin  All rights reserved.
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
namespace beepcore.beep.profile
{
	
	
	/// <summary>The Configuration class (which can be extended of course)
	/// is designed to allow for the provision of profile-specific
	/// settings, as well as the storage of profile-specific data.
	/// 
	/// It can be extended by those implementing profile libraries
	/// at will.</summary>
	public class ProfileConfiguration
	{
		private System.Collections.Specialized.NameValueCollection props = new System.Collections.Specialized.NameValueCollection();
		
		public ProfileConfiguration()
		{
		}
		
		/// <summary>Searches for the property with the specified key in this
		/// ProfileConfiguration.</summary>
		/// <param name="key">the property key.</param>
		/// <returns>the value in this configuration list with the
		/// specified key value or null if it is not found.</returns>
		public virtual string getProperty(string key)
		{
			return props[key];
		}
		
		/// <summary>Searches for the property with the specified key in this
		/// ProfileConfiguration. If the key is not found in this
		/// ProfileConfiguration <code>defaultValue</code> is returned.</summary>
		/// <param name="key">the property key.</param>
		/// <param name="defaultValue">a default value.</param>
		/// <returns>the value in this configuration list with the
		/// specified key value or <code>defaultValue</code>
		/// if it is not found.</returns>
		public virtual string getProperty(string key, string defaultValue)
		{
			return (props[key] == null)?defaultValue:props[key];
		}
		
		/// <summary>Returns an enumeration of all the keys in this ProfileConfiguration.</summary>
		public virtual System.Collections.ICollection propertyNames()
		{
			return props.Keys;
		}
		
		/// <summary>Stores the value with the associated key in this ProfileConfiguration.</summary>
		/// <returns>the previous value of the specified key in this
		/// ProfileConfiguration, or <code>null</code> if it did not have
		/// one.</returns>
		public virtual void setProperty(string key, string value)
		{
			props[key] = value;
		}
	}
}