import React from 'react';
import moment from 'moment';
import $ from 'jquery';
import * as Constants from './Constants'
import PointHistory from './PointHistory';
import SelectTag from './SelectTag';
import PointInput from './PointInput';

class App extends React.Component {
  constructor(){
    super();
    this.state = {
      points: [],
      tags: [],
      selectedTagId: null
    }
  }

  componentDidMount() {
    this.updateHistory();
    this.updateTags();
  }

  updateHistory() {
    $.ajax({
      url: "/api/points",
      data: {
        startDate: moment().subtract(30, 'days').format(Constants.dateFormat)
      },
      dataType:'json',
      cache: false,
      success: function(data){
        this.setState({points: data}, function(){
          console.log(this.state);
        });
      }.bind(this),
      error: function(xhr, status, err){
        console.log(err);
      }.bind(this)
    });
  }

  updateTags() {
    $.ajax({
      url: "/api/tags",
      dataType: "json",
      cache: false,
      success: function(data) {
        this.setState({tags: data}, function() {
          console.log(this.state);
        }).bind(this);
      }.bind(this),
      error: function(xhr, status, err) {
        console.log(err);
      }.bind(this)
    });
  }

  selectTag(tagId) {
    this.setState({selectedTagId: tagId}, function() {
      console.log(this.state);
    });
  }

  resetTag() {
    this.setState({selectedTagId: null}, function() {
      console.log(this.state);
    });
  }

  submitPoint(point) {
    console.log("Submitting point", point);
    $.ajax({
      url: "/api/points",
      dataType: "json",
      contentType:"application/json; charset=utf-8",
      method: "POST",
      data: JSON.stringify(point),
      success: function(data) {
        console.log(data);
        this.resetTag();
        this.updateHistory();
      }.bind(this),
      error: function(xhr, status, err) {
        console.log(err);
        this.resetTag();
      }.bind(this)
    });
  }

  render() {
    var selectTag = <SelectTag
      tags={this.state.tags}
      selectTag={this.selectTag.bind(this)}
    />;
    var pointInput = <PointInput
      tag={this.state.tags.find(tag => tag.id === this.state.selectedTagId)}
      resetTag={this.resetTag.bind(this)}
      submitPoint={this.submitPoint.bind(this)}
    />;
    return (
      <div>
        {this.state.selectedTagId === null ? selectTag : pointInput}
        <PointHistory points={this.state.points} />
      </div>
    );
  }
}

export default App;
