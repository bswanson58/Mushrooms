using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;
using Mushrooms.Support;

namespace Mushrooms.ColorAnimation
{
    internal interface IAnimationProcessor {
        IObservable<List<ColorAnimationResult>> OnResultsPublished { get; }

        void StartAnimationProcessor();
        void StopAnimationProcessor();

        void AddJob( ColorAnimationJob job );
        void UpdateJobParameters( string jobId, ColorAnimationParameters parameters );
    }

    internal class ColorAnimationJobItem {
        public ColorAnimationJob Job { get; }
        public List<ColorAnimationSequence> Sequences { get; }
        public List<ColorAnimationResult> LastResults { get; }

        public ColorAnimationJobItem( ColorAnimationJob job ) {
            Job = job;
            Sequences = new List<ColorAnimationSequence>();
            LastResults = new List<ColorAnimationResult>();
        }
    }

    public class ColorAnimationProcessor : IAnimationProcessor {
        private readonly ISequenceFactory mSequenceFactory;
        private readonly List<ColorAnimationJobItem> mJobs;
        private readonly CancellationTokenSource mTokenSource;
        private Task mAnimationTask;
        private readonly Subject<List<ColorAnimationResult>> mResultSubject;

        public IObservable<List<ColorAnimationResult>> OnResultsPublished => mResultSubject.AsObservable();

        public ColorAnimationProcessor( ISequenceFactory sequenceFactory ) {
            mSequenceFactory = sequenceFactory;
            mJobs = new List<ColorAnimationJobItem>();
            mTokenSource = new CancellationTokenSource();
            mAnimationTask = Task.CompletedTask;
            mResultSubject = new Subject<List<ColorAnimationResult>>();
        }

        public void StartAnimationProcessor() {
            mAnimationTask = Repeat.Interval( TimeSpan.FromMilliseconds( 100 ), AnimationTask, mTokenSource.Token );
        }

        public void StopAnimationProcessor() {
            mTokenSource.Cancel();

            mAnimationTask.Wait( 250 );
            mAnimationTask = Task.CompletedTask;
        }

        public void AddJob( ColorAnimationJob job ) {
            mJobs.Add( new ColorAnimationJobItem( job ) );
        }

        public void UpdateJobParameters( string jobId, ColorAnimationParameters parameters ) {
            var job = mJobs.FirstOrDefault(j => j.Job.JobId.Equals(jobId));

            if( job != null ) {
                job.Job.UpdateParameters( parameters );
            }
        }

        private void AnimationTask() {
            foreach( var job in mJobs ) {
                UpdateSequences( job );

                job.LastResults.Clear();

                foreach( var sequence in job.Sequences ) {
                    job.LastResults.Add( sequence.RunAnimationStep( job.Job.Parameters ));
                }

                mResultSubject.OnNext( job.LastResults );
            }
        }

        private void UpdateSequences( ColorAnimationJobItem job ) {
            var completedList = job.Sequences.Where(s => s.CouldTransition).ToList();

            foreach( var sequence in completedList ) {
                job.Sequences.Remove( sequence );
            }

            foreach( var bulb in job.Job.BulbGroup ) {
                if( !job.Sequences.Any( s => s.Bulbs.Any( b => b.Equals( bulb )))) {
                    var bulbList = job.Job.Parameters.SynchronizedBulbs ? job.Job.BulbGroup : new List<string> { bulb };
                    var pattern = job.Job.GetRandomPattern();

                    job.Sequences.Add( mSequenceFactory.Create( bulbList, pattern ));
                }
            }
        }
    }
}
