using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Xaml.Behaviors;

namespace ReusableBits.Wpf.Behaviours {
	// usage:
	//
	//<ItemsControl ... >
	//<i:Interaction.Behaviors>
	//    <Behaviours:EmptyListBehavior
	//				EmptyTemplate="templateName"
	//				ProgressTemplate="templateName"
	//				IsUpdating="boolean value" />
	//</i:Interaction.Behaviors>
	//</ItemsControl>
	public class EmptyListBehavior : Behavior<ItemsControl> {
		private ControlTemplate ? 	mDefaultTemplate;
		private ItemsControl ?		mItemsControl;
		private bool				mDefaultTemplateCollected;

		public static readonly DependencyProperty EmptyTemplateProperty = DependencyProperty.Register(
			nameof( EmptyTemplate ),
			typeof( ControlTemplate ),
			typeof( EmptyListBehavior ),
			new PropertyMetadata( null ) );

		public ControlTemplate ? EmptyTemplate {
			get => GetValue( EmptyTemplateProperty ) as ControlTemplate;
            set => SetValue( EmptyTemplateProperty, value );
        }

		public static readonly DependencyProperty ProgressTemplateProperty = DependencyProperty.Register(
			nameof( ProgressTemplate ),
			typeof( ControlTemplate ),
			typeof( EmptyListBehavior ),
			new PropertyMetadata( null ) );

		public ControlTemplate ? ProgressTemplate {
			get => GetValue( ProgressTemplateProperty ) as ControlTemplate;
            set => SetValue( ProgressTemplateProperty, value );
        }

		public static readonly DependencyProperty IsUpdatingProperty = DependencyProperty.Register(
			nameof( IsUpdating ),
			typeof( bool ),
			typeof( EmptyListBehavior ),
			new PropertyMetadata( false, IsUpdatingChanged ));

		public bool IsUpdating {
			get => (bool)GetValue( IsUpdatingProperty );
            set => SetValue( IsUpdatingProperty, value );
        }

		private static void IsUpdatingChanged( DependencyObject d, DependencyPropertyChangedEventArgs e ) {
            if(( d is EmptyListBehavior behavior ) &&
               ( behavior.mItemsControl != null )) {
				behavior.SetTemplate();
			}
		}

		private void SetTemplate() {
			if( mDefaultTemplateCollected ) {
				if(( IsUpdating ) &&
				   ( mItemsControl != null ) &&
				   ( ProgressTemplate != null )) {
					mItemsControl.Template = ProgressTemplate;
				}
				else {
					if(( mItemsControl?.HasItems == false ) &&
					   ( EmptyTemplate != null )) {
						mItemsControl.Template = EmptyTemplate;
					}
					else {
						if(( mDefaultTemplate != null ) &&
						   ( mItemsControl != null )) {
							mItemsControl.Template = mDefaultTemplate;
						}
					}
				}
			}
		}

		private void OnLoaded( object sender, RoutedEventArgs args ) {
			if( mItemsControl != null ) {
                mItemsControl.Loaded -= OnLoaded;

                mDefaultTemplate = mItemsControl.Template;
                mDefaultTemplateCollected = true;

                SetTemplate();
            }
		}

		private void OnCollectionChanged( object ? sender, NotifyCollectionChangedEventArgs args ) {
			SetTemplate();
		}

		protected override void OnAttached() {
			base.OnAttached();

			mItemsControl = AssociatedObject;

			if( mItemsControl != null ) {
				mItemsControl = AssociatedObject;

				mItemsControl.Loaded += OnLoaded;
				if( mItemsControl?.Items != null ) {
					((INotifyCollectionChanged)( mItemsControl.Items )).CollectionChanged += OnCollectionChanged;
				}
			}
		}

		protected override void OnDetaching() {
			base.OnDetaching();

			if( mItemsControl != null ) {
				mItemsControl.Loaded -= OnLoaded;

				if( mItemsControl?.Items != null ) {
					((INotifyCollectionChanged)( mItemsControl.Items )).CollectionChanged -= OnCollectionChanged;
				}
			}
		}
	}
}