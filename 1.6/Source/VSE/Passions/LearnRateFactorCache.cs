using System.Linq;
using System.Runtime.CompilerServices;
using RimWorld;
using Verse;

namespace VSE.Passions;

public static class LearnRateFactorCache
{
    private static ConditionalWeakTable<SkillRecord, CacheData> cache = new();

    public static float LearnRateFactorBase(this SkillRecord sr) => cache.GetValue(sr, sr => new CacheData { Value = GetValueFor(sr) }).Value;

    public static void ClearCacheFor(SkillRecord sr, Passion? other = null)
    {
        cache.Remove(sr);
        if (sr.passion.IsCritical() || (other.HasValue && other.Value.IsCritical()))
            foreach (var record in sr.pawn.skills.skills)
                cache.Remove(record);
    }

    public static void ClearCache() => cache = new ConditionalWeakTable<SkillRecord, CacheData>();

    private static float GetValueFor(SkillRecord sr)
    {
        var passionDef = PassionManager.PassionToDef(sr.passion);
        if (SkillsMod.Settings.CriticalEffectPassions || passionDef.isBad)//IF CriticalEffectPassions is enabled OR this is the Apathy passion
            return sr.pawn.skills.skills//Get this pawn's skills
               .Except(sr)//Except the current skill, i.e. critical passions should not apply learnRateFactorOther to themselves.
               .Aggregate(passionDef.learnRateFactor,
                    (current, skillRecord) => current * PassionManager.PassionToDef(skillRecord.passion).learnRateFactorOther);
        else if (SkillsMod.Settings.AlternateCriticalEffects)
            if (passionDef.defName === "Minor")
                return sr.pawn.skills.skills
                    .Except(sr)
                    .Aggregate(passionDef.learnRateFactor,//current = learnRateFactor of this skill
                        (current, skillRecord) => current * PassionManager.PassionToDef(skillRecord.passion).learnRateFactorOtherAltMinor);
            else if (passionDef.defName === "Major")
                return sr.pawn.skills.skills
                    .Except(sr)
                    .Aggregate(passionDef.learnRateFactor,
                        (current, skillRecord) => current * PassionManager.PassionToDef(skillRecord.passion).learnRateFactorOtherAltMajor);
            else if (passionDef.defName === "VSE_Natural")
                return sr.pawn.skills.skills
                    .Except(sr)
                    .Aggregate(passionDef.learnRateFactor,
                        (current, skillRecord) => current * PassionManager.PassionToDef(skillRecord.passion).learnRateFactorOtherAltNatural);
            else if (passionDef.defName === "VSE_Critical")
                return sr.pawn.skills.skills
                    .Except(sr)
                    .Aggregate(passionDef.learnRateFactor,
                        (current, skillRecord) => current * PassionManager.PassionToDef(skillRecord.passion).learnRateFactorOtherAltCritical);

        return passionDef.learnRateFactor;//ELSE return the defined learnRateFactor
    }

    private class CacheData
    {
        public float Value;
    }
}
