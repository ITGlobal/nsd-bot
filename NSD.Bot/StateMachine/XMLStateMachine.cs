using System;
using System.Diagnostics;
using System.Xml;

namespace NSD.Bot {
    /// <remarks>
/// This class implements a table-driven finite state machine.
/// The table is defined by an XML document. The System.Xml.XmlTextReader 
/// class is used for fast scanning of the table and allows larger tables 
/// to be used as opposed to System.Xml.XmlDocument.
/// </remarks>
	class XMLStateMachine {
		public XMLStateMachine() {
			tableParser = null;
			stateCurrent = String.Empty;
			stateTable = String.Empty;
			_answer = String.Empty;
		}

/// <summary>
/// The CurrentState property contains the current state in the table.
/// </summary>
		public string CurrentState {
			get {
				return stateCurrent;
			}
			set {
				stateCurrent = value;
			}
		}

/// <summary>
/// The Answer property contains a user-defined string
/// that indicates an _answer to be performed on the current transition.
/// </summary>
		public string Answer {
			get {
				return _answer;
			}
		}	

/// <summary>
/// The StateTable property contains the state table file name.
/// </summary>
		public string StateTable {
			get {
				return stateTable;
			}
			set {
				stateTable = value;
			}
		}	

/// <summary>
/// The Next method gets the next valid state given
/// the current state and the supplied input.
/// </summary>
/// <param name="inputArg">The input used to trigger a state transition.</param>
/// <returns>A string that identifies the next state</returns>
		public string Next(string inputArg) {
			string nextState = String.Empty;

			if ( CurrentState != String.Empty ) {
				try {
					tableParser = new XmlTextReader(StateTable);

					while ( true == tableParser.Read() ) {
						if ( XmlNodeType.Element == tableParser.NodeType ) {
							if ( true == tableParser.HasAttributes ) {
								string state = tableParser.GetAttribute("name");
								if ( state == CurrentState ) {
									// Get transition data
									while ( true == tableParser.Read() ) {
										if ( ( XmlNodeType.Element == tableParser.NodeType ) && ("transition" == tableParser.Name) ) {
											if ( true == tableParser.HasAttributes ) {
												string input = tableParser.GetAttribute("input");
												if ( input == inputArg ) {
													CurrentState = tableParser.GetAttribute("next");
													nextState = CurrentState;
													_answer = tableParser.GetAttribute("_answer");
													return nextState;
												}
											}
										}
									}
								}
							}
						}
					}
				}
				catch (XmlException e) {
					// Eliminate default trace listener
					Trace.Listeners.RemoveAt(0);
					// Add console trace listener
					TextWriterTraceListener myWriter = new TextWriterTraceListener(System.Console.Out);
	 				Trace.Listeners.Add(myWriter);
					Trace.WriteLine("[XMLStateMachine] Could not load state table definition.");
					Trace.Indent();
					Trace.WriteLine(e.Message);
					Trace.Unindent();
					// 	Clean up object
					tableParser.Close();
					tableParser = null;
					stateCurrent = String.Empty;
					_answer = String.Empty;
				}
			}

			return nextState;
		}

		private XmlTextReader tableParser;
		private string stateCurrent;
		private string stateTable;
		private string _answer;
	}
}