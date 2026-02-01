using System;
using GGJ.Gameplay.Faces;
using GGJ.Mapping;
using TinyGoose.Tremble;
using UnityEngine;

namespace GGJ.Gameplay.Characters
{
    [PrefabEntity("character")]
    public class FaceCharacter : TriggerSender
    {
        [SerializeField, Tremble("person")] private FaceTextureCollection faceTextureCollection;
        [SerializeField, Tremble("expression")] private FaceExpression expression = FaceExpression.Joy;
        [SerializeField, Tremble("dialogue")] private DialogueSequence dialogueSequence;

        [SerializeField, NoTremble] private Face face;
        [SerializeField, NoTremble] private CharacterInfo characterInfo;
        [SerializeField, NoTremble] private Animator animator;

        private void Awake()
        {
            if (animator)
                animator.SetTrigger(expression.ToString());
        }

        public override void OnImportFromMapEntity(MapBsp mapBsp, BspEntity entity)
        {
            if (face && faceTextureCollection)
                face.SetFace(faceTextureCollection.GetFromExpression(expression));

            if (characterInfo && dialogueSequence)
                characterInfo.sequence = dialogueSequence;
            
            if (animator)
                animator.SetTrigger(expression.ToString());
        }
    }
}