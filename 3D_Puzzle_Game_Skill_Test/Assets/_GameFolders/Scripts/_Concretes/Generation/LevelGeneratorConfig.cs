using UnityEngine;
using System;
using System.Collections.Generic;
using BufoGames.Data;

namespace BufoGames.Generation
{
    [Serializable]
    public class LevelGeneratorConfig
    {
        [Header("Grid")]
        [Range(2, 12)] public int gridWidth = 4;
        [Range(2, 12)] public int gridHeight = 4;
        
        [Header("Difficulty")]
        [Range(1, 10)] public int targetDifficulty = 5;
        
        [Header("Destinations")]
        [Range(1, 10)] public int destinationCount = 1;
        
        [Header("Allowed Pieces")]
        public bool useStraightPipes = true;
        public bool useCornerPipes = true;
        public bool useTJunctionPipes = true;
        public bool useCrossPipes = false;
        public bool useStaticPipes = false;
        [Range(0f, 0.5f)] public float staticPipeRatio = 0.2f;
        
        [Header("Advanced")]
        [Range(0f, 0.5f)] public float decoyPipeRatio = 0.1f;
        [Range(0, 100)] public int seed = 0;
        public bool useRandomSeed = true;
        
        public List<PieceType> GetAllowedPipeTypes()
        {
            var types = new List<PieceType>();
            if (useStraightPipes) types.Add(PieceType.StraightPipe);
            if (useCornerPipes) types.Add(PieceType.CornerPipe);
            if (useTJunctionPipes) types.Add(PieceType.TJunctionPipe);
            if (useCrossPipes) types.Add(PieceType.CrossPipe);
            return types;
        }
        
        public int GetTargetMinMoves()
        {
            int baseMoves = targetDifficulty switch
            {
                1 => 1,
                2 => 2,
                3 => 3,
                4 => 5,
                5 => 7,
                6 => 9,
                7 => 12,
                8 => 15,
                9 => 18,
                10 => 22,
                _ => 5
            };
            return baseMoves + (destinationCount - 1) * 3;
        }
        
        public float GetPathComplexity()
        {
            return Mathf.Clamp01(targetDifficulty / 10f);
        }
        
        /// <summary>
        /// Applies a difficulty preset that configures all parameters for the given difficulty level.
        /// Grid size, pipe types, ratios, and destination count are all set automatically.
        /// </summary>
        public void ApplyDifficultyPreset(int difficulty)
        {
            difficulty = Mathf.Clamp(difficulty, 1, 10);
            targetDifficulty = difficulty;
            
            switch (difficulty)
            {
                case 1: // Tutorial
                    gridWidth = 3; gridHeight = 3;
                    destinationCount = 1;
                    useStraightPipes = true; useCornerPipes = true;
                    useTJunctionPipes = false; useCrossPipes = false;
                    useStaticPipes = true; staticPipeRatio = 0.4f;
                    decoyPipeRatio = 0f;
                    break;
                case 2:
                    gridWidth = 3; gridHeight = 3;
                    destinationCount = 1;
                    useStraightPipes = true; useCornerPipes = true;
                    useTJunctionPipes = false; useCrossPipes = false;
                    useStaticPipes = true; staticPipeRatio = 0.3f;
                    decoyPipeRatio = 0f;
                    break;
                case 3:
                    gridWidth = 4; gridHeight = 3;
                    destinationCount = 1;
                    useStraightPipes = true; useCornerPipes = true;
                    useTJunctionPipes = true; useCrossPipes = false;
                    useStaticPipes = true; staticPipeRatio = 0.2f;
                    decoyPipeRatio = 0.05f;
                    break;
                case 4:
                    gridWidth = 4; gridHeight = 4;
                    destinationCount = 1;
                    useStraightPipes = true; useCornerPipes = true;
                    useTJunctionPipes = true; useCrossPipes = false;
                    useStaticPipes = true; staticPipeRatio = 0.15f;
                    decoyPipeRatio = 0.1f;
                    break;
                case 5: // Medium
                    gridWidth = 5; gridHeight = 4;
                    destinationCount = 1;
                    useStraightPipes = true; useCornerPipes = true;
                    useTJunctionPipes = true; useCrossPipes = false;
                    useStaticPipes = false; staticPipeRatio = 0f;
                    decoyPipeRatio = 0.1f;
                    break;
                case 6:
                    gridWidth = 5; gridHeight = 5;
                    destinationCount = 2;
                    useStraightPipes = true; useCornerPipes = true;
                    useTJunctionPipes = true; useCrossPipes = false;
                    useStaticPipes = false; staticPipeRatio = 0f;
                    decoyPipeRatio = 0.15f;
                    break;
                case 7:
                    gridWidth = 6; gridHeight = 5;
                    destinationCount = 2;
                    useStraightPipes = true; useCornerPipes = true;
                    useTJunctionPipes = true; useCrossPipes = true;
                    useStaticPipes = false; staticPipeRatio = 0f;
                    decoyPipeRatio = 0.15f;
                    break;
                case 8:
                    gridWidth = 6; gridHeight = 6;
                    destinationCount = 2;
                    useStraightPipes = true; useCornerPipes = true;
                    useTJunctionPipes = true; useCrossPipes = true;
                    useStaticPipes = false; staticPipeRatio = 0f;
                    decoyPipeRatio = 0.2f;
                    break;
                case 9:
                    gridWidth = 7; gridHeight = 6;
                    destinationCount = 3;
                    useStraightPipes = true; useCornerPipes = true;
                    useTJunctionPipes = true; useCrossPipes = true;
                    useStaticPipes = false; staticPipeRatio = 0f;
                    decoyPipeRatio = 0.25f;
                    break;
                case 10: // Expert
                    gridWidth = 8; gridHeight = 7;
                    destinationCount = 3;
                    useStraightPipes = true; useCornerPipes = true;
                    useTJunctionPipes = true; useCrossPipes = true;
                    useStaticPipes = false; staticPipeRatio = 0f;
                    decoyPipeRatio = 0.3f;
                    break;
            }
        }
    }
}
