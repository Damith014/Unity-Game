using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Collections;

public  interface IPlayerRespawnListener 
{
    void OnPlayerRespawnInThisCheckpoint(CheckPoint chekpoint, Player player);

}

