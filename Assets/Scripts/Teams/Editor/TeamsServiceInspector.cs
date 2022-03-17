//// --------------------------------------------------------------------------------------------------------------------
//// <copyright file="PunTeamsInspector.cs" company="Exit Games GmbH">
////   Part of: Photon Unity Utilities, 
//// </copyright>
//// <summary>
////  Custom inspector for PunTeams
//// </summary>
//// <author>developer@exitgames.com</author>
//// --------------------------------------------------------------------------------------------------------------------

//using Photon.Realtime;
//using System.Collections.Generic;
//using UnityEditor;

//namespace Photon.Pun.UtilityScripts
//{
//    [CustomEditor(typeof(TeamsManager))]
//	public class PunTeamsInspector : Editor {

//		private Dictionary<Team, bool> foldouts;

//		public override void OnInspectorGUI()
//		{
//			if (foldouts == null)
//			{
//				foldouts = new Dictionary<Team, bool>();
//			}

//			if (TeamsManager.PlayersPerTeam != null)
//			{
//				foreach (KeyValuePair<Team,List<PhotonNetworkedPlayer>> pair in TeamsManager.PlayersPerTeam)
//				{	
//					if (!foldouts.ContainsKey(pair.Key))
//					{
//						foldouts[pair.Key] = true;
//					}

//					foldouts[pair.Key] =   EditorGUILayout.Foldout(foldouts[pair.Key], "Team "+ pair.Key +" ("+pair.Value.Count+")");

//					if (foldouts[pair.Key])
//					{
//						EditorGUI.indentLevel++;
//						foreach(PhotonNetworkedPlayer player in pair.Value)
//						{
//							EditorGUILayout.LabelField("", player.ToString() + (PhotonNetwork.LocalPlayer == player ? " - You -" : ""));
//						}
//						EditorGUI.indentLevel--;
//					}
				
//				}
//			}
//		}
//	}
//}