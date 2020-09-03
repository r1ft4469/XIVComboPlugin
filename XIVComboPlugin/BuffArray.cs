using Dalamud.Game;
using Dalamud.Game.ClientState;
using Dalamud.Game.ClientState.Structs;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using XIVComboPlugin.JobActions;

namespace XIVComboPlugin
{
    public class BuffArray
    {

        public bool SearchTarget(short needle,ClientState clientState, int owner = 0, float duration = 0)
        {
            if (owner == 0)
                owner = clientState.LocalPlayer.ActorId;
            if (clientState.Targets.CurrentTarget != null)
            {
                if (clientState.Targets.CurrentTarget.statusEffects.Length > 0)
                {
                    var statusEffects = clientState.Targets.CurrentTarget.statusEffects;
                    for (var i = 0; i < statusEffects.Length; i++)
                    {
                        if (statusEffects[i].EffectId == needle && statusEffects[i].OwnerId == owner && statusEffects[i].Duration > duration)
                            return true;
                    }
                }
            }
            return false;
        }
        public bool SearchPlayer(short needle, ClientState clientState, int owner = 0, float duration = 0)
        {
            if (owner == 0)
                owner = clientState.LocalPlayer.ActorId;
            if (clientState.LocalPlayer != null)
            {
                if (clientState.LocalPlayer.statusEffects.Length > 0)
                {
                    var statusEffects = clientState.LocalPlayer.statusEffects;
                    for (var i = 0; i < statusEffects.Length; i++)
                    {
                        if (statusEffects[i].EffectId == needle && statusEffects[i].OwnerId == owner && statusEffects[i].Duration > duration)
                            return true;
                    }
                }
            }
            return false;
        }
    }
}
