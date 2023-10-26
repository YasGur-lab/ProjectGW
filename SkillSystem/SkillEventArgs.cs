using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillEventArgs : EventArgs
{
    public Skill skill { get; private set; }
    public float cooldown { get; private set; }
    public SkillEventArgs(Skill skill) {  this.skill = skill; }

    public SkillEventArgs(Skill skill, float cooldown)
    {
        this.skill = skill;
        this.cooldown = cooldown;
    }
    // Start is called before the first frame update

    public delegate void SkillActivatedEventHandler(object sender, SkillEventArgs args);
    public delegate void SkillCooldownUpdateEventHandler(object sender, SkillEventArgs args);
    public delegate void SkillSkillCompletedEventHandler(object sender, SkillEventArgs args);
    public delegate void SkillCooldownCompletedEventHandler(object sender, SkillEventArgs args);
}
