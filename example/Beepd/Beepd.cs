/*
* Beepd.java  $Revision: 1.1 $ $Date: 2004/09/21 08:47:01 $
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
using System.Collections;
using System.Threading;
using Log = beepcore.logging.Log;
using LogFactory = beepcore.logging.LogFactory;
using ProfileRegistry = beepcore.beep.core.ProfileRegistry;
using SessionTuningProperties = beepcore.beep.core.SessionTuningProperties;
using beepcore.beep.profile;
using TCPSessionCreator = beepcore.beep.transport.tcp.TCPSessionCreator;
using Element = System.Xml.XmlElement;
using Node = System.Xml.XmlNode;
using NodeList = System.Xml.XmlNodeList;
using Document = System.Xml.XmlDocument;
namespace beepcore.beep.example
{
	
	/// <summary> Sample BEEP server analogous to inetd. Based on the configuration file
	/// it loads the specified profiles and listens for new sessions on the
	/// specified ports.
	/// 
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
	/// <version>  $Revision: 1.1 $, $Date: 2004/09/21 08:47:01 $
	/// </version>
	public class Beepd
	{
		private int port;
		internal ProfileRegistry reg;
		
		private Log log = LogFactory.getLog(typeof(Beepd));
		
		
		/// <summary> Parses the beepd element in the configuration file and loads the
		/// classes for the specified profiles.
		/// 
		/// </summary>
		/// <param name="serverConfig">&ltbeepd&gt configuration element.
		/// </param>
		private Beepd(Element serverConfig)
		{
			reg = new ProfileRegistry();
			
			if (serverConfig.HasAttribute("port") == false)
			{
				throw new System.Exception("Invalid configuration, no port specified");
			}
			
			port = System.Int32.Parse(serverConfig.GetAttribute("port"));
			
			// Parse the list of profile elements.
			NodeList profiles = serverConfig.GetElementsByTagName("profile");
			for (int i = 0; i < profiles.Count; ++i)
			{
				if (profiles[i].NodeType != System.Xml.XmlNodeType.Element)
				{
					continue;
				}
				
				Element profile = (Element) profiles[i];
				
				if (profile.Name.ToUpper().Equals("profile".ToUpper()) == false)
				{
					continue;
				}
				
				string uri;
				string className;
				//string requiredProperites;
				//string tuningProperties;
				
				if (profile.HasAttribute("uri") == false)
				{
					throw new System.Exception("Invalid configuration, no uri specified");
				}
				
				uri = profile.GetAttribute("uri");
				
				if (profile.HasAttribute("class") == false)
				{
					throw new System.Exception("Invalid configuration, no class " + "specified for profile " + uri);
				}
				
				className = profile.GetAttribute("class");
				
				// parse the parameter elements into a ProfileConfiguration
				ProfileConfiguration profileConfig = parseProfileConfig(profile.GetElementsByTagName("parameter"));
				
				// load the profile class
				IProfile p;
				try
				{
					p = (IProfile) new beepcore.beep.profile.echo.EchoProfile(); //SupportClass.CreateNewInstance(System.Type.GetType(className));
					//p = (IProfile) Activator.CreateInstance(Type.GetType(className, true, true));
				}
				catch (System.InvalidCastException)
				{
					throw new System.Exception("class " + className + " does not " + "implement the " + "beepcore.beep.profile.Profile " + "interface");
				}
				catch (System.Exception e)
				{
					throw new System.Exception("Class " + className + " not found - " + e.ToString());
				}
				
				SessionTuningProperties tuning = null;
				
				if (profile.HasAttribute("tuning"))
				{
					string tuningString = profile.GetAttribute("tuning");
					System.Collections.Hashtable hash = new System.Collections.Hashtable();
					string[] tokens = tuningString.Split(":=".ToCharArray());
					
					for(int tki=0; tki+1<tokens.Length; tki+=2) {
						string parameter = tokens[tki];
						string paramvalue = tokens[tki+1];
						
						hash[parameter] = paramvalue;
					}
					
					tuning = new SessionTuningProperties(hash);
				}
				
				// Initialize the profile and add it to the advertised profiles
				reg.addStartChannelListener(uri, p.Init(uri, profileConfig), tuning);
			}

			thread = new Thread(new ThreadStart(this.ThreadProc));
		}
		
		[STAThread]
		public static void Main(string[] argv)
		{
			System.IO.FileInfo config = new System.IO.FileInfo("config.xml");
			
			for (int i = 0; i < argv.Length; ++i)
			{
				if (argv[i].ToUpper().Equals("-config".ToUpper()))
				{
					config = new System.IO.FileInfo(argv[++i]);
					bool tmpBool;
					if (System.IO.File.Exists(config.FullName))
						tmpBool = true;
					else
						tmpBool = System.IO.Directory.Exists(config.FullName);
					if (tmpBool == false)
					{
						System.Console.Error.WriteLine("Beepd: Error file " + config.FullName + " does not exist");
						return ;
					}
				}
				else
				{
					System.Console.Error.WriteLine(usage);
					return ;
				}
			}
			
			Document doc = new Document();
			try
			{
				doc.Load(config.FullName);
			}
			catch (System.Xml.XmlException e)
			{
				System.Console.Error.WriteLine("Beepd: Error parsing config\n" + e.ToString());
				return ;
			}
			catch (System.IO.IOException e)
			{
				System.Console.Error.WriteLine("Beepd: Error parsing config\n" + e.Message);
				return ;
			}
			
			// Parse the configuration file

			System.Collections.ICollection servers;
			try
			{
				servers = parseConfig(doc.DocumentElement);
			}
			catch (System.Exception e)
			{
				System.Console.Error.WriteLine(e.Message);
				return ;
			}
			
			// Start the servers listening
			foreach(Beepd srv in servers) 
			{
				srv.Start();
			}
			
			System.Console.Out.WriteLine("Beepd: started");

			foreach(Beepd srv in servers) 
			{
				srv.Join();
			}

			System.Console.Out.WriteLine("Beepd: all servers finished");
		}

		private Thread thread;

		public void Start()
		{
			thread.Start();
		}

		public void Join()
		{
			thread.Join();
		}

		private void ThreadProc()
		{
			try
			{
				// Loop listening for new Sessions
				while (true)
				{
					TCPSessionCreator.listen(port, reg);
				}
			}
			catch (System.Exception e)
			{
				log.error("Listener exiting", e);
			}
		}
		
		private static System.Collections.ICollection parseConfig(Element doc)
		{
			ArrayList servers = new ArrayList();
			NodeList serverNodes = doc.ChildNodes;
			
			for (int i = 0; i < serverNodes.Count; ++i)
			{
				Node s = serverNodes[i];

				if (s.NodeType != System.Xml.XmlNodeType.Element )
				{
					continue;
				}
				
				if (s.Name.ToUpper().Equals("beepd".ToUpper()) == false)
				{
					continue;
				}
				
				servers.Add(new Beepd((Element) s));
			}
			
			return servers;
		}
		
		/// <summary> Parses the child elements of a profile element into a
		/// <code>ProfileConfiguration</code> object.
		/// 
		/// </summary>
		/// <param name="profileConfig">list of parameter child elements of a profile
		/// element.
		/// </param>
		//UPGRADE_TODO: Interface 'w3c.dom.NodeList' was not converted. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1095"'
		private static ProfileConfiguration parseProfileConfig(NodeList profileConfig)
		{
			ProfileConfiguration config = new ProfileConfiguration();
			
			//UPGRADE_TODO: Method 'w3c.dom.NodeList.getLength' was not converted. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1095"'
			for (int i = 0; i < profileConfig.Count; ++i)
			{
				//UPGRADE_TODO: Interface 'w3c.dom.Element' was not converted. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1095"'
				//UPGRADE_TODO: Method 'w3c.dom.NodeList.item' was not converted. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1095"'
				Element parameter = (Element) profileConfig[i];
				
				//UPGRADE_TODO: Method 'w3c.dom.Element.HasAttribute' was not converted. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1095"'
				if (parameter.HasAttribute("name") == false || parameter.HasAttribute("value") == false)
				{
					throw new System.Exception("Invalid configuration parameter " + "missing name or value attibute");
				}
				
				//UPGRADE_TODO: Method 'w3c.dom.Element.GetAttribute' was not converted. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1095"'
				config.setProperty(parameter.GetAttribute("name"), parameter.GetAttribute("value"));
			}
			
			return config;
		}
		
		private const string usage = "usage: beepd [-config <file>]\n\n" + "options:\n" + "    -config file  File to read the configuration from.\n";
	}
}