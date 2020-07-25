﻿using System.Collections.Generic;
using Exiled.API.Features;
using Exiled.Events.EventArgs;
using MEC;

namespace DoorRestartSystem
{
	class EventHandlers
	{
		private CoroutineHandle coroutine;
		private List<Door> brokenDoors = new List<Door>();
		private List<Door> doors = new List<Door>();
		private bool isRestarting = false;
		private bool isRoundStarted = false;

		private const float delay = 15.03f;

		public void OnRoundRestart()
		{
			Timing.KillCoroutines(coroutine);
			brokenDoors.Clear();
			doors.Clear();
			isRestarting = false;
			isRoundStarted = false;
		}

		public void OnRoundStart()
		{
			isRoundStarted = true;
			coroutine = Timing.RunCoroutine(StartSystem());
		}

		public void OnRoundEnd(RoundEndedEventArgs ev) => isRoundStarted = false;

		private IEnumerator<float> BreakDoor(Door door)
		{
			doors.Remove(door);
			brokenDoors.Add(door);
			yield return Timing.WaitForSeconds(0.7f);
			if (isRestarting)
			{
				door.NetworkisOpen = !door.NetworkisOpen;
				door.Networklocked = !door.Networklocked;
			}
			doors.Add(door);
			brokenDoors.Remove(door);
		}

		private IEnumerator<float> StartSystem()
		{
			while (isRoundStarted)
			{
				yield return Timing.WaitForSeconds(UnityEngine.Random.Range(480, 660));
				if (UnityEngine.Random.Range(0, 100) < 50)
				{
					foreach (Door door in Map.Doors) doors.Add(door);
					if (!Warhead.IsInProgress && !Warhead.IsDetonated)
					{
						isRestarting = true;
						Timing.CallDelayed(delay, () => isRestarting = false);
						Cassie.Message("CRITICAL ERROR . . DOOR SYSTEM MALFUNCTION IN PROGRESS . . DOOR SYSTEM SOFTWARE REPAIR COMMENCING IN 3 . 2 . 1 . . . . . . . DOOR SYSTEM REPAIR COMPLETE", true, true);
						List<Door> openDoors = new List<Door>();
						foreach (Door door in Map.Doors) if (door.NetworkisOpen) openDoors.Add(door);
						while (isRestarting)
						{
							Door door = doors[UnityEngine.Random.Range(0, doors.Count)];
							Timing.RunCoroutine(BreakDoor(door));
							yield return Timing.WaitForSeconds(0.05f);
						}
						foreach (Door door in Map.Doors)
						{
							door.NetworkisOpen = false;
							door.Networklocked = true;
						}
						yield return Timing.WaitForSeconds(3f);
						foreach (Door door in Map.Doors)
						{
							door.NetworkisOpen = openDoors.Contains(door);
							door.Networklocked = false;
						}
						brokenDoors.Clear();
					}
				}
			}
		}
	}
}
