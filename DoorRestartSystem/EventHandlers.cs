using System.Collections.Generic;
using System.Linq;
using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.Events.EventArgs;
using Interactables.Interobjects.DoorUtils;
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
			Timing.KillCoroutines(new CoroutineHandle[] { coroutine });
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
				door.IsOpen = !door.IsOpen;
				door.ChangeLock(door.IsLocked ? DoorLockType.None : DoorLockType.AdminCommand);
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
					DoorVariant scp106door = DoorNametagExtension.NamedDoors["106_PRIMARY"].TargetDoor;
					DoorVariant scp106door2 = DoorNametagExtension.NamedDoors["106_SECONDARY"].TargetDoor;
					foreach (Door door in Door.Get(x => x.Position != scp106door.transform.position && x.Position != scp106door2.transform.position)) doors.Add(door);

					if (!Warhead.IsInProgress && !Warhead.IsDetonated)
					{
						isRestarting = true;
						Timing.CallDelayed(delay, () => isRestarting = false);
						Cassie.Message("CRITICAL ERROR . . DOOR SYSTEM MALFUNCTION IN PROGRESS . . DOOR SYSTEM SOFTWARE REPAIR COMMENCING IN 3 . 2 . 1 . . . . . . . DOOR SYSTEM REPAIR COMPLETE", true, true);
						List<Door> openDoors = new List<Door>();
						foreach (Door door in Door.List) if (door.IsOpen) openDoors.Add(door);
						while (isRestarting)
						{
							Door door = doors[UnityEngine.Random.Range(0, doors.Count)];
							Timing.RunCoroutine(BreakDoor(door));
							yield return Timing.WaitForSeconds(0.05f);
						}
						foreach (Door door in Door.List)
						{
							door.IsOpen = false;
							door.ChangeLock(DoorLockType.AdminCommand);
						}
						yield return Timing.WaitForSeconds(3f);
						foreach (Door door in Door.List)
						{
							door.IsOpen = openDoors.Contains(door);
							door.ChangeLock(DoorLockType.AdminCommand);
						}
						brokenDoors.Clear();
					}
				}
			}
		}
	}
}
