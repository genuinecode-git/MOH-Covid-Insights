module "network" {
  source = "./modules/network"

  prefix   = local.prefix
  vpc_cidr = var.vpc_cidr
  azs      = local.azs
}