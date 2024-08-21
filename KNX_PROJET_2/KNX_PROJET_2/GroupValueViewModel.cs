using GalaSoft.MvvmLight;
using Knx.Falcon.ApplicationData.DatapointTypes;
using Knx.Falcon.ApplicationData.Editing;
using Knx.Falcon;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KNX_PROJET_2
{
    public class GroupValueViewModel : ViewModelBase
    {
        private GroupValue _value;
        private DptBase _dptConverter;
        private object _convertedValue;
        private KnxEditableValue _editableValue;
        private Exception _conversionError;

        public GroupValueViewModel(GroupValue value)
        {
            Value = value;
        }

        [Browsable(false)]
        public GroupValue Value
        {
            get => _value;
            set
            {
                if (Set(() => Value, ref _value, value))
                {
                    InternalConvert();
                }
            }
        }

        
        [Browsable(true)]
        public object ConvertedValue
        {
            get => _convertedValue;
            set
            {
                Set(() => ConvertedValue, ref _convertedValue, value);
            }
        }

        [Browsable(true)]
        public KnxEditableValue EditableValue
        {
            get => _editableValue;
            set
            {
                if (Set(() => EditableValue, ref _editableValue, value))
                {
                    InternalConvertBack();
                }
            }
        }

        [Browsable(false)]
        public Exception ConversionError => _conversionError;


        private void InternalConvert()
        {
            _convertedValue = null;
            _editableValue = null;
            _conversionError = null;
            if (_dptConverter != null && _value != null)
            {
                try
                {
                    _convertedValue = _dptConverter.ToValue(_value);
                    if (_convertedValue != null)
                        _editableValue = new KnxEditableValue(_convertedValue, _dptConverter);
                }
                catch (Exception exc)
                {
                    _conversionError = exc;
                }
            }

            RaisePropertyChanged(() => ConvertedValue);
            RaisePropertyChanged(() => EditableValue);
            RaisePropertyChanged(() => ConversionError);
        }




        //_________________________________________________________________________________________________________//
        internal void InternalConvertBack()
        {
            _conversionError = null;
            _convertedValue = _editableValue != null ? _editableValue.CreateValueInstance() : null;
            _value = null;
            if (_dptConverter != null && _convertedValue != null)
            {
                try
                {
                    _value = _dptConverter.ToGroupValue(_convertedValue);
                    _conversionError = null;
                }
                catch (Exception exc)
                {
                    _conversionError = exc;
                    _value = null;
                }
            }

            RaisePropertyChanged(() => ConvertedValue);
            RaisePropertyChanged(() => Value);
            RaisePropertyChanged(() => ConversionError);
        }

        public override string ToString()
        {
            return _value.ToString();
        }
    }
}
