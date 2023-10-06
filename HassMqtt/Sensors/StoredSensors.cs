using HassMqtt.Models;
using ReusableBits.Platform.Interfaces;

namespace HassMqtt.Sensors {
    public interface IStoredSensors {
        Task<bool>  LoadAsync();
        Task<bool>  Store();

        AbstractSingleValueSensor ConvertConfiguredToAbstractSingleValue( ConfiguredSensor sensor );
        AbstractMultiValueSensor ConvertConfiguredToAbstractMultiValue( ConfiguredSensor sensor );
    }

    public class StoredSensors : IStoredSensors {
        private readonly IBasicLog  mLog;

        public StoredSensors( IBasicLog log ) {
            mLog = log;
        }

        public Task<bool> LoadAsync() {
            return Task.FromResult( true );
        }

        public Task<bool> Store() {
            return Task.FromResult( true );
        }

        public AbstractSingleValueSensor ConvertConfiguredToAbstractSingleValue( ConfiguredSensor sensor ) {
            AbstractSingleValueSensor abstractSensor = null;

            switch( sensor.Type ) {
                default:
                    mLog.LogMessage( $"[SETTINGS_SENSORS] [{sensor.Name}] Unknown configured single-value sensor type: {sensor.Type.ToString()}" );
                    break;
            }

            return abstractSensor;
        }

        public AbstractMultiValueSensor ConvertConfiguredToAbstractMultiValue( ConfiguredSensor sensor ) {
            AbstractMultiValueSensor abstractSensor = null;

            switch( sensor.Type ) {
//                case SensorType.BatterySensors:
//                    abstractSensor = new BatterySensors(sensor.UpdateInterval, sensor.Name, sensor.Id.ToString());
//                    break;
                default:
                    mLog.LogMessage( $"[SETTINGS_SENSORS] [{sensor.Name}] Unknown configured multi-value sensor type: {sensor.Type.ToString()}" );
                    break;
            }

            return abstractSensor;
        }
    }
}
