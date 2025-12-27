/*
 * Copyright (c) 2025 Yvens R Serpa [https://github.com/YvensFaos/]
 * 
 * This work is licensed under the Creative Commons Attribution 4.0 International License.
 * To view a copy of this license, visit http://creativecommons.org/licenses/by/4.0/
 * or see the LICENSE file in the root directory of this repository.
 */

using UnityEngine;

namespace Actors.AI
{
    [CreateAssetMenu(fileName = "New AIGenes", menuName = "Waves/AI/AI Genes", order = 0)]
    // ReSharper disable once InconsistentNaming
    public class AIGenesSO : ScriptableObject
    {
        [Header("Behavior Genes")]
        public float aggressiveness = 1.0f;
        public float patience = 1.0f;
        public float friendliness = 1.0f;
        public float selfPreservation = 1.0f;
        public float awareness = 1.0f;
        public float sight = 5.0f;

        [Header("Interests")] 
        public float targetInterest = 0.1f;

        [Header("Data")] 
        public int possibleActionsCount = 4;
    }
}