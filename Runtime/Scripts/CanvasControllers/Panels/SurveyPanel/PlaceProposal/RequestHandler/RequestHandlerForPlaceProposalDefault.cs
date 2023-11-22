using SurveyAPI.Service;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

namespace SurveyAPI.CanvasControllers
{
    public class RequestHandlerForPlaceProposalDefault : RequestHandlerForPlaceProposalBase
    {
         [Header("External")]
        [SerializeField] private SurveyServiceBase surveyService;

        [Header("Config")]
        [SerializeField][TextArea] private string sendSuccessLabel = "Sending data OK!";
        [SerializeField][TextArea(3,5)] private string sendErrorLabel = "Sending data failed!\nStatus code: {0}\nmsg: {1}";

        [Header("Events")]
        [SerializeField] private UnityEvent<string> sendSuccessEvent;
        [SerializeField] private UnityEvent<string> sendErrorEvent;


        public override void PostPlaceProposal(double userLat, double userLon, PlaceProposalStoreReqest storeData, IList<byte[]> photos)
        {
            if (storeData == null)
                throw new Exception(GetType().Name + " - storeData cannot be null!");

            StartCoroutine(PostAction(userLat,userLon,storeData,photos));
        }

        private IEnumerator PostAction(double userLat, double userLon, PlaceProposalStoreReqest storeData, IList<byte[]> photos)
        {
            Task<ServerResponse<string>> surveyTask = surveyService.PostPlaceProposalAsync(userLat,userLon,storeData);
            while (surveyTask.IsCompleted == false)
                yield return null;

            if (surveyTask.IsCompletedSuccessfully == true && surveyTask.Result.Value != null)
            {
                string placeProposalID = surveyTask.Result.Value;

                if (photos != null && photos.Count > 0)
                    yield return SendPhotos(userLat,userLon,placeProposalID,photos); else
                    CallSuccess();
            } else
            {
                CallError(surveyTask.Result.StatusCode.ToString(),surveyTask.Result.CustomMessage);
            }
        }
        private IEnumerator SendPhotos(double userLat, double userLon, string placeProposalID, IList<byte[]> photos)
        {
            if (placeProposalID != null && placeProposalID.Length > 0 && photos != null && photos.Count > 0)
            {
                Task<ServerResponse<string>> photoTask = surveyService.PostPlaceProposalSavePhotosAsync(
                    userLat,userLon,placeProposalID,photos);

                while (photoTask.IsCompleted == false)
                    yield return null;

                if (photoTask.IsCompletedSuccessfully == true && photoTask.Result.Value != null)
                    CallSuccess(); else
                    CallError(photoTask.Result.StatusCode.ToString(),photoTask.Result.CustomMessage);
            } else
            {
                throw new Exception(GetType().Name + " - placeProposalID and photos cannot be null or empty");
            }
        }

        private void CallSuccess()
        {
            string text = string.Format(sendSuccessLabel);
            sendSuccessEvent.Invoke(text);
        }
        private void CallError(string statusCode, string customMessage)
        {
            string text = string.Format(sendErrorLabel,statusCode,customMessage);
            sendErrorEvent.Invoke(text);
        }
    }
}
