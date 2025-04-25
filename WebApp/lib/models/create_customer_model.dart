import 'package:json_annotation/json_annotation.dart';

part 'create_customer_model.g.dart';

@JsonSerializable()
class CreateCustomerModel {
  CreateCustomerModel({required this.name, required this.associatedDomains});

  factory CreateCustomerModel.fromJson(Map<String, dynamic> json) =>
      _$CreateCustomerModelFromJson(json);

  final List<String> associatedDomains;
  final String name;

  Map<String, dynamic> toJson() => _$CreateCustomerModelToJson(this);
}
