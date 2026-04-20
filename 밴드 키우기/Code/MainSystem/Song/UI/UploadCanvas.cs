using System;
using Code.Core.Bus;
using Code.Core.Bus.GameEvents;
using Code.MainSystem.Song.Director;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

namespace Code.MainSystem.Song.UI
{
    public class UploadCanvas : MonoBehaviour
    {
        [SerializeField] private GameObject partUI;
        [SerializeField] private UploadTopBar uploadTopBar;
        [SerializeField] private SongDropSlot dropSlot;
        [SerializeField] private SongDataSetUI dataSetUI;
        [SerializeField] private AdjustmentUI adjustmentUI;
        [SerializeField] private FinalResultUI finalResultUI;
        
        
        private void Awake()
        {
            Bus<MusicUploadEvent>.OnEvent += HandleUpload;
            dropSlot.UploadSongReady += HandleSongReady;
            dataSetUI.UploadSongSucceeded += HandleUploadSong;
            adjustmentUI.OnShowResult += HandleShowResult;
            finalResultUI.CloseResult += HandleCloseResult;
            gameObject.SetActive(false);
        }

        private void OnDestroy()
        {
            Bus<MusicUploadEvent>.OnEvent -= HandleUpload;
            dropSlot.UploadSongReady -= HandleSongReady;
            dataSetUI.UploadSongSucceeded -= HandleUploadSong;
            adjustmentUI.OnShowResult -= HandleShowResult;
            finalResultUI.CloseResult -= HandleCloseResult;
        }

        private void Start()
        {
            // dropSlot.gameObject.SetActive(false);
            partUI.gameObject.SetActive(false);
            dataSetUI.gameObject.SetActive(false);
            adjustmentUI.gameObject.SetActive(false);
            finalResultUI.gameObject.SetActive(false);
        }
        
        private void HandleUpload(MusicUploadEvent evt)
        {
            dropSlot.Reset();
            dataSetUI.Reset();
            adjustmentUI.Reset();
            finalResultUI.Reset();
            gameObject.SetActive(true);
            dropSlot.gameObject.SetActive(true);
            partUI.gameObject.SetActive(false);
        }
        
        private void HandleSongReady()
        {
            uploadTopBar.UpdateCurIndex(0);
            partUI.gameObject.SetActive(true);
            dataSetUI.gameObject.SetActive(true);
        }
        
        private void HandleUploadSong()
        {
            uploadTopBar.UpdateCurIndex(1);
            dataSetUI.gameObject.SetActive(false);
            adjustmentUI.Open();
        }
        
        private void HandleShowResult()
        {
            uploadTopBar.UpdateCurIndex(2);
            adjustmentUI.Hide();
            finalResultUI.gameObject.SetActive(true);

        }
        
        private void HandleCloseResult()
        {
            finalResultUI.gameObject.SetActive(false);
            gameObject.SetActive(false);
        }
    }
}