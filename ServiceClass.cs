using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using System.ServiceProcess;
using System.Security.Principal;
using System.Runtime.InteropServices;
using Microsoft.VisualBasic;

namespace WindowsFormsApplication1
{
    class ServiceClass
    {
		/// <summary>
		/// Process user command.
		/// </summary>
		/// <param name="args">Command arguments.</param>
        [STAThread]
		public static String ExecuteService( string[] args ) {			
			
            if ( ! args[ 1 ].StartsWith( "/" ) ) {							

				string user = null, password = null, domain = null;
				int timeout = int.MinValue;

				for( int i = 3; i < args.Length; i++ ) {

					// handle timeout argument					
					if ( args[ i ].ToUpper().StartsWith( Args.TIMEOUT ) ) {
						try {
							timeout = int.Parse( args[ 3 ].Substring( Args.TIMEOUT.Length + 1) );
						} catch( FormatException e ) {
                            throw new Exception("Invalid timeout argument. " + e.Message);
						}
					
					// handle user name and password argument
					} else if ( args[ i ].ToUpper().StartsWith( Args.USER ) ) {
						user = args[ i ].Substring( Args.USER.Length + 1 );
					} else if ( args[ i ].ToUpper().StartsWith( Args.PASSWORD ) ) {
						password = args[ i ].Substring( Args.PASSWORD.Length + 1 );
					} else if ( args[ i ].ToUpper().StartsWith( Args.DOMAIN ) ) {
						domain = args[ i ].Substring( Args.DOMAIN.Length + 1 );
					}
				}

				// impersonate if needs to
				if ( user != null && password != null && domain != null ) {
					if ( ! ImpersonationUtil.Impersonate( user, password, domain ) ) {
                        throw new Exception("No such account found, Impersonation failed. LoginFailed");
					}
				}

                try
                {
                    ServiceController service = new ServiceController(args[1], args[0]);

                    // handle action argument
                    if (args[2].ToUpper().Equals("/GETSTATUS"))
                    {
                        return service.Status.ToString(); ;
                    }
                    else if (args[2].ToUpper().Equals(Args.RESTART))
                    {
                        RestartService(service, timeout);
                    }
                    else if (args[2].ToUpper().Equals(Args.START))
                    {
                        StartService(service, timeout);
                    }
                    else if (args[2].ToUpper().Equals(Args.STOP))
                    {
                        StopService(service, timeout);
                    }
                    else if (args[2].ToUpper().Equals(Args.PAUSE))
                    {
                        PauseService(service, timeout);
                    }
                    else if (args[2].ToUpper().Equals(Args.CONTINUE))
                    {
                        ContinueService(service, timeout);
                    }
                    else
                    {
                        throw new Exception("No such action : " + args[2]);
                    }

                }
                catch (Exception e)
                {
                    throw new Exception(e.Message);
                }
                finally
                {
					// undo impersonation 
					if ( user != null && password != null && domain != null ) {
						ImpersonationUtil.UnImpersonate();
					}
				}

            }
            else {
                throw new Exception("Invalid Parameters");
            }
            return null;
		}

		/// <summary>
		/// Restart a service.
		/// This action will in turn call StopService and StartService.
		/// If the service is not currently stopped, it will try to stop the service first.
		/// </summary>
		/// <param name="service">service controller.</param>
		/// <param name="timeout">timeout value used for stopping and restarting.</param>
		private static void RestartService( ServiceController service, int timeout ) {
			if ( ServiceControllerStatus.Stopped != service.Status ) {
				StopService( service, timeout );
			}
			StartService( service, timeout );
		}

		/// <summary>
		/// Start a service.
		/// </summary>
		/// <param name="service">service controller.</param>
		/// <param name="timeout">timeout value for starting.</param>
		private static void StartService( ServiceController service, int timeout ) {
			if ( ServiceControllerStatus.Stopped == service.Status ) 
            {
				service.Start();
				if ( int.MinValue != timeout ) {
					TimeSpan t = TimeSpan.FromSeconds( timeout );
					service.WaitForStatus( ServiceControllerStatus.Running, t );
                } 
                else 
                {
                    service.WaitForStatus(ServiceControllerStatus.Running);
                }			
			} 
            else 
            {
                throw new Exception("Cannot start service '" + service.ServiceName + "' on '" + service.MachineName + "' because it is not stopped.");
			}
		}

		/// <summary>
		/// Pause a service.
		/// </summary>
		/// <param name="service">service controller.</param>
		/// <param name="timeout">timeout value for pausing.</param>
		private static void PauseService( ServiceController service, int timeout ) {
			if ( service.CanPauseAndContinue ) 
            {
			    service.Pause();
			    if ( int.MinValue != timeout ) {
				    TimeSpan t = TimeSpan.FromSeconds( timeout );
				    service.WaitForStatus( ServiceControllerStatus.Paused, t );
			    } 
                else 
                {
                    service.WaitForStatus( ServiceControllerStatus.Paused );
                }		
			} 
            else
            {
                throw new Exception("Service's 'CanPauseAndContinue' property is set to false. Cannot pause service '" + service.ServiceName + "' on '" + service.MachineName + "'");		
            }
		}

		/// <summary>
		/// Continue a service.
		/// </summary>
		/// <param name="service">service controller.</param>
		/// <param name="timeout">timeout value for continuing.</param>
		private static void ContinueService( ServiceController service, int timeout ) {
			if ( service.CanPauseAndContinue ) 
            {
				service.Continue();
                if (int.MinValue != timeout)
                {
                    TimeSpan t = TimeSpan.FromSeconds(timeout);
                    service.WaitForStatus(ServiceControllerStatus.Running, t);
                } 
                else 
                {
                    service.WaitForStatus(ServiceControllerStatus.Running);
                }	
			} 
            else 
            {
                throw new Exception("Service's 'CanPauseAndContinue' property is set to false. Cannot continue service '" + service.ServiceName + "' on '" + service.MachineName + "'");
			}
		}

		/// <summary>
		/// Stop a service.
		/// </summary>
		/// <param name="service">service controller.</param>
		/// <param name="timeout">timeout for stopping the service.</param>
		private static void StopService( ServiceController service, int timeout ) {
			if ( service.CanStop ) 
            {
				service.Stop();
                if (int.MinValue != timeout)
                {
                    TimeSpan t = TimeSpan.FromSeconds(timeout);
                    service.WaitForStatus(ServiceControllerStatus.Stopped, t);
                }
                else
                {
                    service.WaitForStatus(ServiceControllerStatus.Stopped);
                }
			} 
            else 
            {
                throw new Exception("Cannot stop service '" + service.ServiceName + "' on '" + service.MachineName + "'");
			}
		}
	}

	public class Args {
		public const string STATUS		= "/STATUS";
		public const string RESTART		= "/RESTART";
		public const string STOP		= "/STOP";
		public const string START		= "/START";
		public const string PAUSE		= "/PAUSE";
		public const string CONTINUE	= "/CONTINUE";
		public const string TIMEOUT		= "/TIMEOUT";
		public const string USER		= "/USER";
		public const string PASSWORD	= "/PASSWORD";
		public const string DOMAIN		= "/DOMAIN";
	}

	/// <summary>
	/// Impersonate a windows logon.
	/// </summary>
	public class ImpersonationUtil {

		/// <summary>
		/// Impersonate given logon information.
		/// </summary>
		/// <param name="logon">Windows logon name.</param>
		/// <param name="password">password</param>
		/// <param name="domain">domain name</param>
		/// <returns></returns>
		public static bool Impersonate( string logon, string password, string domain ) {
			WindowsIdentity tempWindowsIdentity;
			IntPtr token = IntPtr.Zero;
			IntPtr tokenDuplicate = IntPtr.Zero;

            if (LogonUser(logon, domain, password, LOGON_TYPE_NEW_CREDENTIALS, LOGON32_PROVIDER_WINNT50, ref token) != 0)
            {
                if (DuplicateToken(token, 2, ref tokenDuplicate) != 0)
                {
                    tempWindowsIdentity = new WindowsIdentity(tokenDuplicate);
                    impersonationContext = tempWindowsIdentity.Impersonate();
                    if (null != impersonationContext) return true;
                }
            }
			return false;
		}

		/// <summary>
		/// Unimpersonate.
		/// </summary>
		public static void UnImpersonate() {
			impersonationContext.Undo();
		} 

		[DllImport("advapi32.dll", CharSet=CharSet.Auto)]
		public static extern int LogonUser( 
			string lpszUserName, 
			String lpszDomain,
			String lpszPassword,
			int dwLogonType, 
			int dwLogonProvider,
			ref IntPtr phToken );

		[DllImport("advapi32.dll", CharSet=System.Runtime.InteropServices.CharSet.Auto, SetLastError=true)]
		public extern static int DuplicateToken(
			IntPtr hToken, 
			int impersonationLevel,  
			ref IntPtr hNewToken );


        const int LOGON_TYPE_NEW_CREDENTIALS = 9;
        const int LOGON32_PROVIDER_WINNT50 = 3;
		private static WindowsImpersonationContext impersonationContext; 
	
    }
}
