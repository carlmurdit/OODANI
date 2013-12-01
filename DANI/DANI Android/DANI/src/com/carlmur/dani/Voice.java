package com.carlmur.dani;

import java.util.List;
import android.app.Activity;
import android.content.Intent;
import android.content.pm.PackageManager;
import android.content.pm.ResolveInfo;
import android.speech.RecognizerIntent;
import android.speech.tts.TextToSpeech;


public class Voice extends Activity {
	
	public static final int VOICE_RECOGNITION_CODE = 999; //code for Voice Recognition support on user device
	public int TTS_DATA_CHECK_CODE = 0; //Code checking phone's TTS engine data
	public TextToSpeech repeatTTS; //Text To Speech instance
	
	public Intent CheckTTSDataIntent(PackageManager packManager, Activity act) throws Exception
	{
		//find out whether speech recognition is supported
		List<ResolveInfo> intActivities = packManager.queryIntentActivities(new Intent(RecognizerIntent.ACTION_RECOGNIZE_SPEECH), 0);
		if (intActivities.size() != 0) {
		    //prepare the TTS to repeat chosen words
		    Intent checkTTSIntent = new Intent();
		    //check TTS data
		    checkTTSIntent.setAction(TextToSpeech.Engine.ACTION_CHECK_TTS_DATA);
		    return checkTTSIntent;
		}
		else
		{
		    throw new Exception ("Sorry, speech recognition is not supported on this device.");
		}
	}
		
	public Intent StartListeningIntent()
	{
	    //start the speech recognition intent passing required data
	    Intent listenIntent = new Intent(RecognizerIntent.ACTION_RECOGNIZE_SPEECH);
	    //indicate package
	    listenIntent.putExtra(RecognizerIntent.EXTRA_CALLING_PACKAGE, getClass().getPackage().getName());
	    //message to display while listening
	    listenIntent.putExtra(RecognizerIntent.EXTRA_PROMPT, "Talk to DANI!");
	    //set speech model
	    listenIntent.putExtra(RecognizerIntent.EXTRA_LANGUAGE_MODEL, RecognizerIntent.LANGUAGE_MODEL_FREE_FORM);
	    //specify number of results to retrieve
	    listenIntent.putExtra(RecognizerIntent.EXTRA_MAX_RESULTS, 1);
	    return listenIntent;   
	}
	
	public void TextToSpeech(String msg)
	{
		repeatTTS.speak(msg, TextToSpeech.QUEUE_FLUSH, null);
	}

}
