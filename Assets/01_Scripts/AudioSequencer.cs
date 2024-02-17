using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using UnityEngine;

public class AudioSequencer : MonoBehaviour
{
    [SerializeField] AudioClip[] bassClips;
    [SerializeField] AudioClip[] hiHatClips;
    [SerializeField] AudioClip[] snareClips;

    private AudioClip[][] clips;

    [SerializeField] private int amountOfLines = 3;
    [SerializeField] private int lineLenght = 4;
    [SerializeField] private int bpm = 120;
    [SerializeField] private int sequenceRepeatCount = 5;
    [SerializeField] private bool infiniteRepeat = false;
    [SerializeField] AudioSource[] audioSources;

    private List<SequencerLine> lines = new();

    private async void Start ()
    {
        audioSources = new AudioSource[amountOfLines];

        clips = new AudioClip[amountOfLines][];
        clips[0] = bassClips;
        clips[1] = hiHatClips;
        clips[2] = snareClips;

        SetupAudioSource();
        uint sequenceIndex = 0;

    StartSequence:

        //Waits for the sequence to continue, then checks if it will start another one or ends the function.
        await PlaySequence();
        sequenceIndex++;
        if ( (sequenceIndex < sequenceRepeatCount && !infiniteRepeat) || infiniteRepeat )
        {
            goto StartSequence;
        }
    }

    private float pan = -1;
    private void SetupAudioSource ()
    {
        pan = -1;
        for ( int i = 0; i < amountOfLines; i++ )
        {
            var source = gameObject.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.panStereo = pan;
            pan *= -1;

            audioSources[i] = source;
        }
    }

    private async Task PlaySequence ()
    {
        for ( int i = 0; i < amountOfLines; i++ )
        {
            SequencerLine line = new(lineLenght, bpm, clips[i]);
            lines.Add(line);
            audioSources[i].panStereo = pan;
            pan *= -1;
        }

        Task[] tasks = new Task[lines.Count];
        for ( int i = 0; i < lines.Count; i++ )
        {
            tasks[i] = PlaySequencerAsync(lines[i], audioSources[i]);
        }
        lines.Clear();

        await Task.WhenAll(tasks);
    }

    //Has an odd space in between the first and second note.
    private async Task PlaySequencerAsync ( SequencerLine line, AudioSource source )
    {
        foreach ( var note in line.line )
        {
            source.clip = note.clip;
            source.Play();
            await Awaitable.WaitForSecondsAsync(1 / (line.Bpm / 60.0f));
        }
    }
}

//For storing potentially more info in each note.
public struct SequencerNote
{
    public AudioClip clip;

    public SequencerNote ( AudioClip clip )
    {
        this.clip = clip;
    }
}

public struct SequencerLine
{
    public SequencerNote[] line;
    public float Bpm;

    public SequencerLine ( int lenght, float bpm, AudioClip[] clips )
    {
        line = new SequencerNote[lenght];

        for ( int i = 0; i < line.Length; i++ )
        {
            var clip = clips[Random.Range(0, clips.Length)];
            line[i] = new SequencerNote(clip);
        }

        Bpm = bpm;
    }
}