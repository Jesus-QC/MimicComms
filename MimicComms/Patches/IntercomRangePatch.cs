using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using NorthwoodLib.Pools;
using PlayerRoles;
using PlayerRoles.PlayableScps.Scp939;
using PlayerRoles.Voice;
using PluginAPI.Events;
using VoiceChat;

namespace MimicComms.Patches;

[HarmonyPatch(typeof(Intercom), nameof(Intercom.CheckPlayer))]
public class IntercomRangePatch
{
    private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
    {
        List<CodeInstruction> newInstructions = ListPool<CodeInstruction>.Shared.Rent(instructions);

        Label skip = generator.DefineLabel();
        Label skipFor939 = generator.DefineLabel();
        
        int offset = newInstructions.FindIndex(x => x.opcode == OpCodes.Brfalse_S);
        int offset2 = newInstructions.FindIndex(x => x.opcode == OpCodes.Isinst) + 4;
        
        newInstructions[offset + 1].labels.Add(skip);
        newInstructions[offset].opcode = OpCodes.Br_S;
        newInstructions[offset2].labels.Add(skipFor939);
        
        newInstructions.InsertRange(offset, new CodeInstruction[]
        {
            new (OpCodes.Brtrue_S, skip),
            new (OpCodes.Ldarg_0),
            new (OpCodes.Ldloc_0),
            new (OpCodes.Call, AccessTools.Method(typeof(IntercomRangePatch), nameof(CheckPlayer))),
            new (OpCodes.Brtrue_S, skipFor939),
        });

        foreach (CodeInstruction instruction in newInstructions)
            yield return instruction;

        ListPool<CodeInstruction>.Shared.Return(newInstructions);
    }

    private static bool CheckPlayer(Intercom intercom, PlayerRoleBase roleBase) =>
        roleBase is Scp939Role role && (role.FpcModule.Position - intercom._worldPos).sqrMagnitude < intercom._rangeSqr && role.VoiceModule.ServerIsSending;
}