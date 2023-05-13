using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackData {
    public float damage;
    public float hitStun;
    public bool multiHit;
    Player attackSource;
    public int attackId;
    public static int attackIdFlow = 0;
    //restricts attacks to only hitting once per attack, unless multihit is true
    public int damageInst = 0;
    //restricts attacks to only hitting once per hitbox
    public static int damageInstFlow = 0;

    public AttackData(float newDamage, float newHitStun, bool newMultiHit, Player newAttackSource, int newAttackId) {
        damage = newDamage;
        hitStun = newHitStun;
        multiHit = newMultiHit;
        attackSource = newAttackSource;
        attackId = newAttackId;

        damageInst = damageInstFlow;
        damageInstFlow++;
    }
}
