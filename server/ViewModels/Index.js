var dateStr = "date";
var dateFormat = "YYYY-MM-DD";
var timeFormat = "HH:mm:ss"

var app = new Vue({
  el: '#app',
  data: {
    message: 'Hello Vue!',
    message2: 'Message2',
    tags: [],
    selectedTag: null,
    pointInput: null,
    point: null,
  },
  created: function() {
    var resource = this.$resource('api/tags');
    resource.get().then(response => {
      this.tags = response.body;
    }, response => {
      this.message = "fail";
    });
  },
  methods: {
    selectTag: function(tag) {
      this.pointInput = [];
      this.point = {};
      for (var field of tag.fields) {
        var fieldInput = new FieldInput(field);
        this.pointInput.push(fieldInput);
        this.$set(this.point, field.name, null);
      }
      var dateInput = new FieldInput({ name: dateStr, type: dateStr });
      this.pointInput.push(dateInput);
      dateInput.value = moment().format(dateFormat);
      this.$set(this.point, dateStr, null);
      this.parseDate(dateInput, dateInput.value);
      this.$set(this.point, "tagId", tag.id); // tag id doesn't need input
      this.selectedTag = tag;
    },
    resetTag: function() {
      this.selectedTag = null;
      this.pointInput = null;
      this.point = null;
    },
    incrementDate: function(field) {
      var date = moment(field.value);
      if (!date.isValid()) {
        date = moment();
      }
      var result = date.add(1, 'days').format(dateFormat);
      field.value = result;
      this.parseDate(field, field.value);
    },
    decrementDate: function(field) {
      var date = moment(field.value);
      if (!date.isValid()) {
        date = moment();
      }
      var result = date.subtract(1, 'days').format(dateFormat);
      field.value = result;
      this.parseDate(field, field.value);
    },
    parseInt: function(field, value) {
      field.value = value;
      var int = parseInt(Number(field.value));
      var success = !Number.isNaN(int);
      var result = success ? int : null;
      field.validationMessage = success ? "" : `${field.value} is not an int`;
      this.$set(this.point, field.tagField.name, result);
    },
    parseFloat: function(field, value) {
      field.value = value;
      var float = Number(field.value);
      var success = Number.isFinite(float);
      var result = success ? float : null;
      field.validationMessage = success ? "" : `${field.value} is not a float`;
      this.$set(this.point, field.tagField.name, result);
    },
    parseString: function(field, value) {
      field.value = value;
      var success = !!field.value.length
      var result = success ? field.value : null;
      field.validationMessage = success ? "" : "value cannot be empty";
      this.$set(this.point, field.tagField.name, result);
    },
    parseDate: function(field, value) {
      field.value = value;
      var date = moment(field.value);
      var success = date.isValid();
      var result = success ? date.format(dateFormat) : null;
      field.validationMessage = success ? "" : `${field.value} is not a date`;
      this.$set(this.point, field.tagField.name, result);
    },
    parseTime: function(field, value) {
      field.value = value;
      var time = moment(field.value, timeFormat);
      var success = time.isValid();
      var result = success ? time.format(timeFormat) : null;
      field.validationMessage = success ? "" : `${field.value} is not a time`;
      this.$set(this.point, field.tagField.name, result);
    },
    parseEnum: function(field, value) {
      field.value = value;
      var success = field.tagField.values.includes(field.value);
      var result = success ? field.value : null;
      field.validationMessage = success ? "" : `${field.value} doesn't belong to this enum`;
      this.$set(this.point, field.tagField.name, result);
    },
    submit: function() {
      for (field in this.point) {
        if (this.point[field] === null) {
          return;
        }
      }
      var resource = this.$resource('api/points');
      resource.save(this.point).then(response => {
        this.message = response.body;
        this.resetTag();
      }, response => {
        this.message = response.body;
        this.resetTag();
      })
      this.message2 = JSON.stringify(this.point);
    }
  }
});

class FieldInput {
  constructor(tagField) {
    this.tagField = tagField;
    this.value = "";
    this.validationMessage = "";
  }
}