using GGJ.Gameplay.Faces;
using TinyGoose.Tremble;
using UnityEngine;

namespace GGJ.Gameplay.Characters
{
    [PrefabEntity("character")]
    public class FaceCharacter : MonoBehaviour, IOnImportFromMapEntity
    {
        [SerializeField, Tremble("person")] private FaceTextureCollection faceTextureCollection;
        [SerializeField, Tremble("expression")] private FaceExpression expression = FaceExpression.Joy;

        [SerializeField, NoTremble] private Face face;

        public void OnImportFromMapEntity(MapBsp mapBsp, BspEntity entity)
        {
            if (!face || !faceTextureCollection)
                return;
            
            face.SetFace(faceTextureCollection.GetFromExpression(expression));
        }
    }
}